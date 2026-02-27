namespace DigiXanh.API.Helpers;

/// <summary>
/// Helper methods cho xử lý query string
/// </summary>
public static class QueryStringHelper
{
    /// <summary>
    /// Parse query string và giữ nguyên giá trị URL-encoded (không decode)
    /// Điều này cần thiết để validate VNPay signature chính xác
    /// </summary>
    public static Dictionary<string, string> ParseRaw(string? queryString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return result;
        }

        var query = queryString.TrimStart('?');
        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = parts[0];
            var value = parts.Length > 1 ? parts[1] : string.Empty;
            result[key] = value;
        }

        return result;
    }
}
