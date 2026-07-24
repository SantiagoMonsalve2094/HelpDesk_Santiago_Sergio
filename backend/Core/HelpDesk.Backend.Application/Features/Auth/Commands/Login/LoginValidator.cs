using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Exceptions;
using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.Features.Auth;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Auth.Commands.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .MaximumLength(254)
            .EmailAddress();
        RuleFor(command => command.Password).NotEmpty();
    }
}
