using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
using UserManagementApi.Data;
using UserManagementApi.Services.UserService;
using UserManagementApi.Models.User;
using UserManagementApi.Entities;

namespace UserManagement.UnitTests.Services {
    public class UserServiceTests : IDisposable {
        private readonly DataContext _context;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockJwtSection;
        private readonly UserService _userService;

        public UserServiceTests() {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataContext(options);

            // Setup configuration mocks
            _mockConfiguration = new Mock<IConfiguration>();
            _mockJwtSection = new Mock<IConfigurationSection>();
            
            _mockJwtSection.Setup(x => x["Key"]).Returns("ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLongForTesting!");
            _mockJwtSection.Setup(x => x["Issuer"]).Returns("TestIssuer");
            _mockJwtSection.Setup(x => x["Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(_mockJwtSection.Object);

            _userService = new UserService(_context, _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetAllUsers_WhenUsersExist_ReturnsAllUsers() {
            // Arrange
            var users = new List<User> {
                new User { Id = 1, Username = "user1", Email = "user1@test.com", FirstName = "John", LastName = "Doe" },
                new User { Id = 2, Username = "user2", Email = "user2@test.com", FirstName = "Jane", LastName = "Smith" }
            };
            
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetAllUsers();

            // Assert
            result.Should().HaveCount(2);
            result[0].Username.Should().Be("user1");
            result[1].Username.Should().Be("user2");
        }

        [Fact]
        public async Task GetAllUsers_WhenNoUsers_ReturnsEmptyList() {
            // Act
            var result = await _userService.GetAllUsers();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserById_WithValidId_ReturnsUser() {
            // Arrange
            var user = new User { 
                Id = 1, 
                Username = "testuser", 
                Email = "test@test.com", 
                FirstName = "Test", 
                LastName = "User" 
            };
            
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserById(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Username.Should().Be("testuser");
            result.Email.Should().Be("test@test.com");
        }

        [Fact]
        public async Task GetUserById_WithInvalidId_ReturnsNull() {
            // Act
            var result = await _userService.GetUserById(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AddUser_WithValidData_CreatesUser() {
            // Arrange
            var request = new AddUserRequest {
                Username = "newuser",
                Email = "new@test.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = await _userService.AddUser(request);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("newuser");
            result.Email.Should().Be("new@test.com");
            result.FirstName.Should().Be("New");
            result.LastName.Should().Be("User");

            // Verify user was actually saved to database
            var savedUser = await _context.Users.FindAsync(result.Id);
            savedUser.Should().NotBeNull();
            BCrypt.Net.BCrypt.Verify("password123", savedUser!.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task AddUser_WithExistingUsername_ThrowsInvalidOperationException() {
            // Arrange
            var existingUser = new User { 
                Username = "existing", 
                Email = "existing@test.com",
                PasswordHash = "hash"
            };

            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            var request = new AddUserRequest {
                Username = "existing", // Same username
                Email = "different@test.com",
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.AddUser(request));
            
            exception.Message.Should().Be("Username already exists");
        }

        [Fact]
        public async Task AddUser_WithExistingEmail_ThrowsInvalidOperationException() {
            // Arrange
            var existingUser = new User { 
                Username = "existing", 
                Email = "existing@test.com",
                PasswordHash = "hash"
            };

            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            var request = new AddUserRequest {
                Username = "different",
                Email = "existing@test.com", // Same email
                Password = "password123",
                FirstName = "Test",
                LastName = "User"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.AddUser(request));
            
            exception.Message.Should().Be("Email already exists");
        }

        [Fact]
        public async Task UpdateUser_WithValidData_UpdatesUser() {
            // Arrange
            var user = new User { 
                Id = 1, 
                Username = "original", 
                Email = "original@test.com",
                FirstName = "Original",
                LastName = "User"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var request = new UpdateUserRequest{
                Username = "updated",
                Email = "updated@test.com",
                FirstName = "Updated",
                LastName = "User"
            };

            // Act
            var result = await _userService.UpdateUser(1, request);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("updated");
            result.Email.Should().Be("updated@test.com");
            result.FirstName.Should().Be("Updated");
            result.LastName.Should().Be("User");
        }

        [Fact]
        public async Task UpdateUser_WithNonExistentId_ReturnsNull(){
            // Arrange
            var request = new UpdateUserRequest {
                Username = "updated",
                Email = "updated@test.com",
                FirstName = "Updated",
                LastName = "User"
            };

            // Act
            var result = await _userService.UpdateUser(999, request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsTrue() {
            // Arrange
            var user = new User { 
                Id = 1, 
                Username = "todelete", 
                Email = "delete@test.com"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.DeleteUser(1);

            // Assert
            result.Should().BeTrue();
            
            // Verify user was actually deleted
            var deletedUser = await _context.Users.FindAsync(1);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUser_WithInvalidId_ReturnsFalse() {
            // Act
            var result = await _userService.DeleteUser(999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Register_WithValidData_CreatesUserAndReturnsToken() {
            // Arrange
            var request = new AddUserRequest {
                Username = "newuser",
                Email = "new@test.com",
                Password = "password123",
                FirstName = "New",
                LastName = "User"
            };

            // Act
            var result = await _userService.Register(request);

            // Assert
            result.Should().NotBeNullOrEmpty();
            
            // Verify user was created
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            user.Should().NotBeNull();
            BCrypt.Net.BCrypt.Verify("password123", user!.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken() {
            // Arrange
            var user = new User {
                Username = "testuser",
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "User"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest {
                Email = "test@test.com",
                Password = "password123"
            };

            // Act
            var result = await _userService.Login(request);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ReturnsNull() {
            // Arrange
            var request = new LoginRequest {
                Email = "nonexistent@test.com",
                Password = "password123"
            };

            // Act
            var result = await _userService.Login(request);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsNull() {
            // Arrange
            var user = new User {
                Username = "testuser",
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequest {
                Email = "test@test.com",
                Password = "wrongpassword"
            };

            // Act
            var result = await _userService.Login(request);

            // Assert
            result.Should().BeNull();
        }

        public void Dispose() {
            _context.Dispose();
        }
    }
}