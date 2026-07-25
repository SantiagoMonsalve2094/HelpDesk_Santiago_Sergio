using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using MediatR;

namespace HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategory;

public sealed record UpdateSupportCategoryCommand(
    Guid ActorUserId,
    Guid SupportCategoryId,
    string Name,
    string Description) : IRequest;
