using FluentValidation;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Exceptions;
using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.Features.Auth;
using MediatR;

namespace HelpDesk.Backend.Application.Features.Auth.Commands.Login;

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
