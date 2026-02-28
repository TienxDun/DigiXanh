namespace DigiXanh.API.Helpers;

public static class ImageUrlSanitizer
{
    private static readonly HashSet<string> BlockedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "bs.plantnet.org"
    };

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

        if (IsBlockedHost(uri.Host))
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

    private static bool IsBlockedHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return true;
        }

        foreach (var blockedHost in BlockedHosts)
        {
            if (host.Equals(blockedHost, StringComparison.OrdinalIgnoreCase) ||
                host.EndsWith($".{blockedHost}", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}