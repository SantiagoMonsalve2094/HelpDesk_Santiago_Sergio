using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategorySla;

public sealed record UpdateSupportCategorySlaCommand(
    Guid ActorUserId,
    Guid SupportCategoryId,
    TicketPriority Priority,
    TimeSpan ResponseTime) : IRequest;
