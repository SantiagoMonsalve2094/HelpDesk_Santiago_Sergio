using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Users;

public sealed record UpdateTechnicianProfileApiRequest(
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int MaxActiveTickets);
