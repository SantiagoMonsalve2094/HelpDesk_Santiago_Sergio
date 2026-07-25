using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Api.DTOs.Tickets;

public sealed record AddTicketCommentApiRequest(string Text);
