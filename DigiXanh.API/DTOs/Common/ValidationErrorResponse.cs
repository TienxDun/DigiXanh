namespace DigiXanh.API.DTOs.Common;

public class ValidationErrorResponse
{
    public ValidationErrorResponse(IDictionary<string, string[]> errors)
    {
        Errors = new Dictionary<string, string[]>(errors, StringComparer.OrdinalIgnoreCase);
    }

    public Dictionary<string, string[]> Errors { get; }
}
