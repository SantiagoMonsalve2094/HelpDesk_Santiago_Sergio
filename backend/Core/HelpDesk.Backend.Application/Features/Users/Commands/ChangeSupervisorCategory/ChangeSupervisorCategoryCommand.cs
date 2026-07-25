using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.ChangeSupervisorCategory;

public sealed record ChangeSupervisorCategoryCommand(
    Guid ActorUserId,
    Guid SupervisorUserId,
    Guid SupportCategoryId) : IRequest;
