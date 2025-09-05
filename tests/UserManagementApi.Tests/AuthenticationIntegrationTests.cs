using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UserManagementApi.Models.User;
using Xunit;

namespace UserManagementApi.IntegrationTests;

public class AuthenticationIntegrationTests : IClassFixture<ApiIntegrationTestFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthenticationIntegrationTests(ApiIntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsJwtToken()
    {
        // Arrange
        var request = new AddUserRequest
        {
            Username = $"testuser_{Guid.NewGuid()}",
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "SecurePass123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("token", out var tokenProperty));
        var token = tokenProperty.GetString();
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Verify it's a JWT token (starts with eyJ)
        Assert.StartsWith("eyJ", token);
    }

    [Theory]
    [InlineData("", "test@example.com", "SecurePass123!", "Username")]
    [InlineData("ab", "test@example.com", "SecurePass123!", "Username")]
    [InlineData("testuser", "invalid-email", "SecurePass123!", "email")]
    [InlineData("testuser", "test@example.com", "weak", "Password")]
    public async Task Register_InvalidData_ReturnsBadRequest(string username, string email, string password, string expectedErrorField)
    {
        // Arrange
        var request = new AddUserRequest
        {
            Username = username,
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        // Check that the error message contains something related to the expected field
        Assert.Contains(expectedErrorField, content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsBadRequest()
    {
        // Arrange - Use a specific username for this test
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var username = $"duplicateuser_{uniqueId}";
        
        var request1 = new AddUserRequest
        {
            Username = username,
            Email = $"user1_{uniqueId}@example.com",
            Password = "SecurePass123!",
            FirstName = "Test",
            LastName = "User"
        };

        var request2 = new AddUserRequest
        {
            Username = username, // Same username
            Email = $"user2_{uniqueId}@example.com", // Different email
            Password = "SecurePass123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Act - Register first user
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", request1);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode); // Ensure first registration works
        
        // Act - Try to register second user with same username
        var response = await _client.PostAsJsonAsync("/api/auth/register", request2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Username", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtToken()
    {
        // Arrange - First register a user
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{uniqueId}@example.com";
        var password = "SecurePass123!";
        
        var registerRequest = new AddUserRequest
        {
            Username = $"loginuser_{uniqueId}",
            Email = email,
            Password = password,
            FirstName = "Login",
            LastName = "User"
        };
        
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("token", out var tokenProperty));
        var token = tokenProperty.GetString();
        Assert.NotNull(token);
        Assert.StartsWith("eyJ", token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}