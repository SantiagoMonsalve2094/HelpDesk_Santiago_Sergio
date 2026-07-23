namespace HelpDesk.Backend.Api.Contracts;

public sealed record LoginRequest(
    string Email,
    string Password);
