using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models.User;
using UserManagementApi.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserManagementApi.Services.UserService;

public class UserService : IUserService {
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;

    public UserService(DataContext context, IConfiguration configuration) {
        _context = context;
        _configuration = configuration;
    }

    public async Task<List<GetUserResponse>> GetAllUsers() {
        var users = await _context.Users.ToListAsync();
        return users.Select(u => new GetUserResponse {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<GetUserResponse?> GetUserById(int id) {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        return new GetUserResponse {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<GetUserResponse> AddUser(AddUserRequest newUser) {
        // Check for existing user first
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == newUser.Username || u.Email == newUser.Email);

        if (existingUser != null) {
            throw new InvalidOperationException(
                existingUser.Username == newUser.Username
                    ? "Username already exists"
                    : "Email already exists");
        }

        try {

            var user = new User{
                Username = newUser.Username,
                Email = newUser.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                FirstName = newUser.FirstName,
                LastName = newUser.LastName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new GetUserResponse {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            };
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true) {
            throw new InvalidOperationException("Username or email already exists");
        }
    }

    public async Task<GetUserResponse?> UpdateUser(int id, UpdateUserRequest updatedUser)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;


        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id != id && (u.Username == updatedUser.Username || u.Email == updatedUser.Email));

        if (existingUser != null) {
            throw new InvalidOperationException(
                existingUser.Username == updatedUser.Username
                    ? "Username already exists"
                    : "Email already exists");
        }

        try {
            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new GetUserResponse {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt
            };
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true) {
            throw new InvalidOperationException("Username or email already exists");
        }
    }

    public async Task<bool> DeleteUser(int id) {

        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> Register(AddUserRequest request) {
        // Check for existing user first
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

        if (existingUser != null) {
            throw new InvalidOperationException(
                existingUser.Username == request.Username
                    ? "Username already exists"
                    : "Email already exists");
        }

        try {
            var user = new User {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreateToken(user);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true) {
            throw new InvalidOperationException("Username or email already exists");
        }
    }

    public async Task<string?> Login(LoginRequest request) {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return CreateToken(user);
    }

    private string CreateToken(User user) {
    var jwtSettings = _configuration.GetSection("Jwt");
    var secretKey = jwtSettings["Key"];
    var issuer = jwtSettings["Issuer"];
    var audience = jwtSettings["Audience"];


    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (environment == "Testing") {
        secretKey = secretKey ?? "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLongForTesting!";
        issuer = issuer ?? "TestIssuer";
        audience = audience ?? "TestAudience";
    }

    if (string.IsNullOrEmpty(secretKey)) {
        throw new InvalidOperationException("JWT secret key is missing in configuration.");
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[] {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.Now.AddHours(24), // Token expires in 24 hours
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}