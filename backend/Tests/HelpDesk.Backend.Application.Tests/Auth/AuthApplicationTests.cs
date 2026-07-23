using HelpDesk.Backend.Application.Common.Exceptions;
using HelpDesk.Backend.Application.Features.Auth.Commands.Login;
using HelpDesk.Backend.Application.Features.Auth.Queries.GetCurrentUser;
using HelpDesk.Backend.Application.Tests.TestDoubles;

namespace HelpDesk.Backend.Application.Tests.Auth;

public sealed class AuthApplicationTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUserProfile()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var user = ApplicationTestData.Technician([category.Id]);
        context.Add(user);
        var tokens = new FakeAccessTokenGenerator();
        var handler = new LoginHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            tokens,
            new LoginValidator());
        using var cancellation = new CancellationTokenSource();

        var response = await handler.Handle(
            new LoginCommand(user.Email.Value, "secret"),
            cancellation.Token);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(user.Id, response.User.Id);
        Assert.Contains(
            category.Id,
            response.User.TechnicianProfile!.SupportCategoryIds);
        Assert.Same(user, tokens.ReceivedUser);
        Assert.Equal("hash-tech", context.PasswordHasher.ReceivedHash);
        Assert.Equal(cancellation.Token, context.Users.ReceivedCancellationToken);
    }

    [Fact]
    public async Task Login_WithWrongPassword_UsesGenericCredentialsError()
    {
        var context = new TestContext();
        var user = ApplicationTestData.User();
        context.Add(user);
        context.PasswordHasher.VerificationResult = false;
        var handler = new LoginHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            new FakeAccessTokenGenerator(),
            new LoginValidator());

        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(
                new LoginCommand(user.Email.Value, "wrong"),
                CancellationToken.None));

        Assert.Contains("email o la contraseña", exception.Message);
    }

    [Fact]
    public async Task Login_WithMissingUser_UsesSameCredentialsError()
    {
        var context = new TestContext();
        var handler = new LoginHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            new FakeAccessTokenGenerator(),
            new LoginValidator());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(
                new LoginCommand("missing@example.com", "wrong"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Login_WithInactiveUser_UsesSameCredentialsError()
    {
        var context = new TestContext();
        var user = ApplicationTestData.User();
        user.Deactivate(ApplicationTestData.Now.AddMinutes(1));
        context.Add(user);
        var handler = new LoginHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            new FakeAccessTokenGenerator(),
            new LoginValidator());

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => handler.Handle(
                new LoginCommand(user.Email.Value, "secret"),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUserTakenFromActorIdentifier()
    {
        var context = new TestContext();
        var user = ApplicationTestData.User();
        context.Add(user);
        var handler = new GetCurrentUserHandler(
            context.UnitOfWork,
            new GetCurrentUserValidator());

        var response = await handler.Handle(
            new GetCurrentUserQuery(user.Id),
            CancellationToken.None);

        Assert.Equal(user.Id, response.Id);
        Assert.Equal(user.Email.Value, response.Email);
    }
}
