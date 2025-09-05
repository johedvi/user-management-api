using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagementApi.Data;

namespace UserManagementApi.IntegrationTests;

public class ApiIntegrationTestFactory : WebApplicationFactory<Program>
{
    private const string TestJwtKey = "ThisIsAVeryLongSecretKeyThatIsAtLeast32CharactersLongForTesting!";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";
    
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Override configuration with test values
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = TestJwtKey,
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=test;"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace database with in-memory version
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DataContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase(databaseName: _databaseName));

            // Configure JWT authentication for testing
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = TestIssuer,
                        ValidAudience = TestAudience,
                        IssuerSigningKey = key
                    };
                });

            services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));

            // Create database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();
            context.Database.EnsureCreated();
        });
    }

    public DataContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DataContext>();
    }
}