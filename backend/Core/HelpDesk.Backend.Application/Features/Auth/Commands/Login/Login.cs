using FluentValidation;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Common.Exceptions;
using HelpDesk.Backend.Application.Features.Auth.Models;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<LoginResponse>;

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

public sealed class LoginHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator accessTokenGenerator,
    IValidator<LoginCommand> validator)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        var token = accessTokenGenerator.Generate(user);
        return new LoginResponse(
            token.AccessToken,
            "Bearer",
            token.ExpiresAtUtc,
            AuthUserMapper.ToResponse(user));
    }
}
