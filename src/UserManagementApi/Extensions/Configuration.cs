using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagementApi.Data;
using UserManagementApi.Services.UserService;
using FluentValidation;

namespace UserManagementApi.Extensions;

public static class Configuration {
    public static void AddServices(this WebApplicationBuilder builder) {
        // Database - only register for non-testing environments
        if (builder.Environment.EnvironmentName != "Testing") {
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        }

        // Services
        builder.Services.AddScoped<IUserService, UserService>();

        // FluentValidation
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        // JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"];

        // Handle testing environment
        if (builder.Environment.EnvironmentName == "Testing") {
            secretKey = secretKey ?? "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLongForTesting!";
        }

        if (string.IsNullOrEmpty(secretKey)) {
            throw new InvalidOperationException("JWT secret key is missing in configuration.");
        }

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "TestIssuer",
                    ValidAudience = jwtSettings["Audience"] ?? "TestAudience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        builder.Services.AddAuthorization();

        // API Documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    public static void UseMiddlewares(this WebApplication app) {
        // Enable Swagger in Development AND Testing
        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing") {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Security & Pipeline
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
    }
}