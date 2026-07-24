using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Common.Authorization;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Users.Commands.UpdateTechnicianProfile;

public sealed class UpdateTechnicianProfileValidator : AbstractValidator<UpdateTechnicianProfileCommand>
{
    public UpdateTechnicianProfileValidator()
    {
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.TechnicianUserId).NotEmpty();
        RuleFor(command => command.SupportCategoryIds).NotEmpty();
        RuleForEach(command => command.SupportCategoryIds).NotEmpty();
        RuleFor(command => command.MaxActiveTickets).GreaterThan(0);
    }
}
