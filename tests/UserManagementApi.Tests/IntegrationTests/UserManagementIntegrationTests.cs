using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UserManagementApi.Models.User;
using Xunit;

namespace UserManagementApi.IntegrationTests;

public class UserManagementIntegrationTests : IClassFixture<ApiIntegrationTestFactory>
{
    private readonly ApiIntegrationTestFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserManagementIntegrationTests(ApiIntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task<(string token, GetUserResponse user)> CreateUserAndGetTokenAsync()
    {
        var request = new AddUserRequest
        {
            Username = $"user_{Guid.NewGuid()}",
            Email = $"user_{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!",
            FirstName = "Test",
            LastName = "User"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        var token = result.GetProperty("token").GetString()!;
        
        // Get the actual user by querying all users and finding ours
        using var tempClient = _factory.CreateClient();
        tempClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var usersResponse = await tempClient.GetAsync("/api/users");
        var usersContent = await usersResponse.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<GetUserResponse>>(usersContent, _jsonOptions);
        
        if (users == null)
            throw new InvalidOperationException("Failed to deserialize users list.");

        var actualUser = users.First(u => u.Username == request.Username);

        return (token, actualUser);
    }

    private async Task<string> GetUserTokenAsync()
    {
        var (token, _) = await CreateUserAndGetTokenAsync();
        return token;
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var request = new AddUserRequest
        {
            Username = $"admin_{Guid.NewGuid()}",
            Email = $"admin_{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!",
            FirstName = "Admin",
            LastName = "User"
        };

        // Register the user first
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Now update the user's role to Admin in the database
        using var context = _factory.GetDbContext();
        var user = context.Users.First(u => u.Username == request.Username);
        user.Role = "Admin";
        context.SaveChanges();
        
        // Login to get a fresh token with admin claims
        var loginRequest = new LoginRequest
        {
            Email = request.Email,
            Password = request.Password
        };
        
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(loginContent);
        return result.GetProperty("token").GetString()!;
    }

    private void SetAuthorizationHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task GetAllUsers_WithoutToken_ReturnsUnauthorized()
    {
        // Clear any existing authorization
        _client.DefaultRequestHeaders.Authorization = null;
        
        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_WithValidToken_ReturnsUsers()
    {
        // Arrange - Create a user (which also gives us a token)
        var (token, createdUser) = await CreateUserAndGetTokenAsync();
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<GetUserResponse>>(content, _jsonOptions);
        
        Assert.NotNull(users);
        // With shared DB, there will likely be multiple users from other tests
        Assert.True(users.Count >= 1, $"Expected at least 1 user, but got {users.Count}");
        
        // Verify our specific user is in the list
        Assert.Contains(users, u => u.Username == createdUser.Username);
    }

    [Fact]
    public async Task GetUserById_WithValidToken_ExistingUser_ReturnsUser()
    {
        // Arrange - Create a user first and get their actual data
        var (token, createdUser) = await CreateUserAndGetTokenAsync();
        SetAuthorizationHeader(token);

        // Act - Use the actual user ID
        var response = await _client.GetAsync($"/api/users/{createdUser.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);
        
        Assert.NotNull(user);
        Assert.Equal(createdUser.Id, user.Id);
        Assert.Equal(createdUser.Username, user.Username);
    }

    [Fact]
    public async Task GetUserById_WithValidToken_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        SetAuthorizationHeader(token);

        // Act - Use a very high ID that shouldn't exist
        var response = await _client.GetAsync("/api/users/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithoutToken_ReturnsUnauthorized()
    {
        // Clear authorization
        _client.DefaultRequestHeaders.Authorization = null;
        
        // Arrange
        var request = new AddUserRequest
        {
            Username = $"newuser_{Guid.NewGuid()}",
            Email = $"newuser_{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithoutToken_ReturnsUnauthorized()
    {
        // Clear authorization
        _client.DefaultRequestHeaders.Authorization = null;
        
        // Arrange
        var request = new UpdateUserRequest
        {
            Username = $"updateduser_{Guid.NewGuid()}",
            Email = $"updated_{Guid.NewGuid()}@example.com",
            FirstName = "Updated",
            LastName = "User"
        };

        // Act - Use a high ID that might exist in shared DB
        var response = await _client.PutAsJsonAsync("/api/users/1", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithoutToken_ReturnsUnauthorized()
    {
        // Clear authorization
        _client.DefaultRequestHeaders.Authorization = null;
        
        // Act - Use a high ID that might exist in shared DB
        var response = await _client.DeleteAsync("/api/users/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithValidToken_CreatesUser()
    {
        // Arrange
        var token = await GetUserTokenAsync();
        SetAuthorizationHeader(token);

        var createRequest = new AddUserRequest
        {
            Username = $"created_{Guid.NewGuid()}",
            Email = $"created_{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!",
            FirstName = "Created",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);
        
        Assert.NotNull(createdUser);
        Assert.Equal(createRequest.Username, createdUser.Username);
        Assert.Equal(createRequest.Email, createdUser.Email);
    }

    [Fact]
    public async Task UpdateUser_WithValidToken_UpdatesUser()
    {
        // Arrange - Create a user first
        var (token, createdUser) = await CreateUserAndGetTokenAsync();
        SetAuthorizationHeader(token);

        var updateRequest = new UpdateUserRequest
        {
            Username = createdUser.Username, // Keep same username
            Email = createdUser.Email,       // Keep same email
            FirstName = "Updated",
            LastName = "Name"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/users/{createdUser.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var updatedUser = JsonSerializer.Deserialize<GetUserResponse>(content, _jsonOptions);
        
        Assert.NotNull(updatedUser);
        Assert.Equal("Updated", updatedUser.FirstName);
        Assert.Equal("Name", updatedUser.LastName);
    }

    [Fact]
    public async Task CompleteUserLifecycle_WithAdminToken_Success()
    {
        // Arrange
        var adminToken = await GetAdminTokenAsync();
        SetAuthorizationHeader(adminToken);

        var createRequest = new AddUserRequest
        {
            Username = $"lifecycle_{Guid.NewGuid()}",
            Email = $"lifecycle_{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!",
            FirstName = "Lifecycle",
            LastName = "Test"
        };

        // Act & Assert - Create User
        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdUser = JsonSerializer.Deserialize<GetUserResponse>(createContent, _jsonOptions);
        Assert.NotNull(createdUser);

        // Act & Assert - Update User
        var updateRequest = new UpdateUserRequest
        {
            Username = createdUser.Username,
            Email = createdUser.Email,
            FirstName = "Updated",
            LastName = "Name"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{createdUser.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // Act & Assert - Delete User (Admin only)
        var deleteResponse = await _client.DeleteAsync($"/api/users/{createdUser.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/users/{createdUser.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}