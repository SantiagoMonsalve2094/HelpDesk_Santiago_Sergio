using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateTechnicianProfile;

public sealed record UpdateTechnicianProfileCommand(
    Guid ActorUserId,
    Guid TechnicianUserId,
    IReadOnlyCollection<Guid> SupportCategoryIds,
    int MaxActiveTickets) : IRequest;
