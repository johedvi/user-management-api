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
                options.AddPolicy("AllowAll", policy => {
                    // Read allowed origins from environment variable
                    var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
                    
                    // If we have specific origins, use them
                    if (allowedOrigins != null && allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    }
                    else
                    {
                        // Fallback to any origin
                        policy.SetIsOriginAllowed(_ => true)
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    }
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
                        
                        // Allow insecure HTTP requests in development/container scenarios
                        // Remove this in production with proper HTTPS
                        options.RequireHttpsMetadata = false;
                    });
            }

            builder.Services.AddAuthorization();

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<AddUserRequestValidator>();
            builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

            var app = builder.Build();

            

            app.UseSwagger();
            app.UseSwaggerUI();
            
            // Comment out HTTPS redirection if using Azure Container Instances
            // If your container is behind Azure Front Door or a reverse proxy, 
            // they will handle HTTPS termination
            // if (!builder.Environment.IsEnvironment("Testing")) {
            //     app.UseHttpsRedirection();
            // }

            // CORS middleware must be before authentication and other middleware that might return responses
            app.UseCors("AllowAll");
            
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.MapGet("/", () => "Users API is running");

            app.Run();
        }
    }
}