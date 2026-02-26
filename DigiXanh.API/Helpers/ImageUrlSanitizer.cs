namespace DigiXanh.API.Helpers;

public static class ImageUrlSanitizer
{
    public static string NormalizeOrEmpty(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return string.Empty;
        }

        var trimmed = imageUrl.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return string.Empty;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return string.Empty;
        }

        return trimmed;
    }

    public static string? NormalizeOrNull(string? imageUrl)
    {
        var normalized = NormalizeOrEmpty(imageUrl);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}