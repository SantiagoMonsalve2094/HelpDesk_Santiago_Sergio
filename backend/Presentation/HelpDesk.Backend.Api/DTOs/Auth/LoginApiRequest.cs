namespace HelpDesk.Backend.Api.DTOs.Auth;

public sealed record LoginApiRequest(
    string Email,
    string Password);
