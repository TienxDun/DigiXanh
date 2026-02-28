using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DigiXanh.API.Tests.Infrastructure;

namespace DigiXanh.API.Tests.Controllers;

public class AdminAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminEndpoint_ReturnsForbidden_ForUserRoleToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await _client.GetAsync("/api/dashboard/admin");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_ReturnsSuccess_ForAdminRoleToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await _client.GetAsync("/api/dashboard/admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminPlants_ReturnsForbidden_ForUserRoleToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        var response = await _client.GetAsync("/api/admin/plants");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminPlants_ReturnsSuccess_ForAdminRoleToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await _client.GetAsync("/api/admin/plants");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
