using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Users;

public sealed record UpdateUserIdentityApiRequest(
    string FullName,
    string Email);
