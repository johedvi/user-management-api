using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagementApi.Data;

namespace UserManagementApi.IntegrationTests;

public class ApiIntegrationTestFactory : WebApplicationFactory<Program> {
    private const string SharedDatabaseName = "SharedTestDb";

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services => {
            // Remove ALL database-related services maybe overkill??
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DataContext) ||
                           d.ServiceType == typeof(DbContextOptions<DataContext>) ||
                           (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                .ToList();

            foreach (var descriptor in descriptorsToRemove) {
                services.Remove(descriptor);
            }

            // Add shared InMemory database (same name = shared across all tests)
            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase(SharedDatabaseName));
        });
    }

    public DataContext GetDbContext() {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DataContext>();
    }

    public void ClearDatabase() {
        using var context = GetDbContext();
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();
    }
}