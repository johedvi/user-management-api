// File: UserManagementApi.IntegrationTests/ApiHealthIntegrationTests.cs
using System.Net;
using Xunit;

namespace UserManagementApi.IntegrationTests;

public class ApiHealthIntegrationTests : IClassFixture<ApiIntegrationTestFactory>
{
    private readonly HttpClient _client;

    public ApiHealthIntegrationTests(ApiIntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RootEndpoint_ReturnsSuccessMessage()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Users API is running", content);
    }

    [Fact]
    public async Task SwaggerEndpoint_IsAccessible()
    {
        // Act
        var response = await _client.GetAsync("/swagger");

        // Assert
        // Should redirect to swagger/index.html
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Redirect);
    }
}