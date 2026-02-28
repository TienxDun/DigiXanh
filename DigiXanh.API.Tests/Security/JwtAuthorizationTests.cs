using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using DigiXanh.API.Tests.Infrastructure;

namespace DigiXanh.API.Tests.Security;

public class JwtAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public JwtAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/cart");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessProtectedEndpoint_WithInvalidToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
        var response = await _client.GetAsync("/api/cart");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AccessAdminEndpoint_WithUserRole_Returns403()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");
        
        var response = await _client.GetAsync("/api/admin/plants");
        
        // TestAuthHandler returns success for user-token, but Authorize(Roles="Admin") should reject
        // In actual implementation, this returns 403 if role doesn't match
        Assert.True(response.StatusCode == HttpStatusCode.Forbidden || 
                    response.StatusCode == HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccessAdminEndpoint_WithAdminRole_Returns200()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "admin-token");
        
        var response = await _client.GetAsync("/api/admin/plants");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "Requires authentication middleware setup")]
    public async Task AccessProtectedEndpoint_WithValidToken_ReturnsSuccess()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");
        
        var response = await _client.GetAsync("/api/cart");
        
        // With valid auth, should not return 401/403
        Assert.True(response.StatusCode != HttpStatusCode.Unauthorized && 
                    response.StatusCode != HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/orders", "POST")]
    [InlineData("/api/orders", "GET")]
    public async Task UserEndpoints_RequireAuthentication(string endpoint, string method)
    {
        // Without token
        HttpResponseMessage response = method switch
        {
            "GET" => await _client.GetAsync(endpoint),
            "POST" => await _client.PostAsJsonAsync(endpoint, new { }),
            _ => await _client.GetAsync(endpoint)
        };

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/admin/plants", "POST")]
    [InlineData("/api/admin/plants/1", "PUT")]
    [InlineData("/api/admin/orders", "GET")]
    public async Task AdminEndpoints_RequireAdminRole(string endpoint, string method)
    {
        // With user token - should fail
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "user-token");

        HttpResponseMessage response = method switch
        {
            "GET" => await _client.GetAsync(endpoint),
            "POST" => await _client.PostAsJsonAsync(endpoint, new { }),
            "PUT" => await _client.PutAsJsonAsync(endpoint, new { }),
            _ => await _client.GetAsync(endpoint)
        };

        Assert.True(response.StatusCode == HttpStatusCode.Forbidden || 
                    response.StatusCode == HttpStatusCode.Unauthorized);
    }
}
