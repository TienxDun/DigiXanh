namespace DigiXanh.API.DTOs.Auth;

public record LoginResponse(string Token, string Id, string Email, string FullName, string Role);