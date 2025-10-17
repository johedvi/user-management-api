using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FluentValidation;
using UserManagementApi.Validators;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore.InMemory;

namespace UserManagementApi {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Configure Swagger with JWT
            builder.Services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });
                
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            if (builder.Environment.IsEnvironment("Testing")) {
                builder.Services.AddDbContext<DataContext>(options =>
                    options.UseInMemoryDatabase("TestDatabase"));
            }
            else {
                builder.Services.AddDbContext<DataContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            }

            // Services
            builder.Services.AddScoped<IUserService, UserService>();

        
            builder.Services.AddCors(options => {
                options.AddDefaultPolicy(policy => {
                    policy.WithOrigins("http://localhost:5173", "https://proud-water-08f614003.1.azurestaticapps.net", "https://subdorsal-jerica-smokelessly.ngrok-free.dev", "http://localhost:7029")
                         .AllowAnyMethod()
                         .AllowCredentials()
                         .AllowAnyHeader()
                         .SetIsOriginAllowed(_ => true); // This is more permissive for debugging
    });
});

            // JWT authentication setup
            if (!builder.Environment.IsEnvironment("Testing")) {
                var jwtSettings = builder.Configuration.GetSection("Jwt");
                var secretKey = jwtSettings["Key"];
                var issuer = jwtSettings["Issuer"];
                var audience = jwtSettings["Audience"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("JWT secret key is missing in configuration.");
                }

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => {
                        options.TokenValidationParameters = new TokenValidationParameters {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = issuer,
                            ValidAudience = audience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                        };
                    });
            }

            builder.Services.AddAuthorization();

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<AddUserRequestValidator>();
            builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

            var app = builder.Build();

            

            app.UseSwagger();
            app.UseSwaggerUI();

            // CORS middleware must be before authentication and other middleware that might return responses
            app.UseCors();
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/", () => "Users API is running");

            app.Run();
        }
    }
}