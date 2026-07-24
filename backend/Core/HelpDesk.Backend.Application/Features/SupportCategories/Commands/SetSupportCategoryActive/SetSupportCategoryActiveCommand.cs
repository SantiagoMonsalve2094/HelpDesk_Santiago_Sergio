using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.SetSupportCategoryActive;

public sealed record SetSupportCategoryActiveCommand(
    Guid ActorUserId,
    Guid SupportCategoryId,
    bool IsActive) : IRequest;
