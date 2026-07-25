using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.CreateSupportCategory;

public sealed record CreateSupportCategoryCommand(
    Guid ActorUserId,
    string Name,
    string Description,
    TimeSpan LowSla,
    TimeSpan MediumSla,
    TimeSpan HighSla,
    TimeSpan CriticalSla) : IRequest<Guid>;
