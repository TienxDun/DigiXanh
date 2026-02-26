using DigiXanh.API.Helpers;

namespace DigiXanh.API.Tests.Controllers;

public class ImageUrlSanitizerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("javascript:alert('xss')")]
    [InlineData("https://bs.plantnet.org/image/o/abc")]
    [InlineData("http://bs.plantnet.org/image/o/abc")]
    public void NormalizeOrEmpty_ReturnsEmpty_ForInvalidOrBlockedUrls(string? input)
    {
        var result = ImageUrlSanitizer.NormalizeOrEmpty(input);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NormalizeOrEmpty_ReturnsTrimmedValue_ForValidHttpUrl()
    {
        var result = ImageUrlSanitizer.NormalizeOrEmpty(" https://images.example.com/plant.jpg ");

        Assert.Equal("https://images.example.com/plant.jpg", result);
    }
}
