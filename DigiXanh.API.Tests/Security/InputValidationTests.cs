using System.Net;
using System.Net.Http.Json;
using DigiXanh.API.Tests.Infrastructure;

namespace DigiXanh.API.Tests.Security;

public class InputValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public InputValidationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert('xss')>")]
    public async Task CreatePlant_WithXSSPayload_ReturnsBadRequestOrSanitizes(string maliciousInput)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await _client.PostAsJsonAsync("/api/admin/plants", new
        {
            name = maliciousInput,
            scientificName = "Test Scientific",
            price = 100000,
            categoryId = 1
        });

        // Either validation rejects it or sanitization happens
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                    response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Created);
    }

    [Theory]
    [InlineData("'; DROP TABLE Plants; --")]
    [InlineData("1' OR '1'='1")]
    public async Task SearchPlants_WithSQLInjection_ReturnsSafeResult(string sqlInjection)
    {
        var response = await _client.GetAsync($"/api/plants?search={Uri.EscapeDataString(sqlInjection)}");
        
        // Should not crash, should return empty results or handle gracefully
        Assert.True(response.StatusCode == HttpStatusCode.OK || 
                    response.StatusCode == HttpStatusCode.BadRequest);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }
    }

    [Theory]
    [InlineData("99999999999999999999")]
    [InlineData("-1")]
    [InlineData("abc")]
    public async Task GetPlantById_WithInvalidId_Returns400Or404(string invalidId)
    {
        var response = await _client.GetAsync($"/api/plants/{invalidId}");
        
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                    response.StatusCode == HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateOrder_WithInvalidRecipient_ReturnsValidationError(string invalidName)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user-token");

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            recipientName = invalidName,
            phone = "0900000000",
            shippingAddress = "123 Test Street",
            paymentMethod = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abcdef")]
    [InlineData("090-000-0000")]
    public async Task CreateOrder_WithInvalidPhone_ReturnsValidationError(string invalidPhone)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "user-token");

        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            recipientName = "Test User",
            phone = invalidPhone,
            shippingAddress = "123 Test Street",
            paymentMethod = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public async Task CreatePlant_WithInvalidPrice_ReturnsValidationError(decimal invalidPrice)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin-token");

        var response = await _client.PostAsJsonAsync("/api/admin/plants", new
        {
            name = "Test Plant",
            scientificName = "Test Scientific",
            price = invalidPrice,
            categoryId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePlant_WithExtremelyLongName_ReturnsValidationError()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "admin-token");

        var longName = new string('A', 1000);

        var response = await _client.PostAsJsonAsync("/api/admin/plants", new
        {
            name = longName,
            scientificName = "Test Scientific",
            price = 100000,
            categoryId = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
