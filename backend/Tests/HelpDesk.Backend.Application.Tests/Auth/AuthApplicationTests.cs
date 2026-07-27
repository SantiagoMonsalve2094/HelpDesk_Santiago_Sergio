using FluentAssertions;
using HelpDesk.Backend.Application.DTOs.Auth;
using HelpDesk.Backend.Application.Interfaces;
using HelpDesk.Backend.Application.Interfaces.Persistence;
using HelpDesk.Backend.Application.Exceptions;
using HelpDesk.Backend.Application.Features.Auth.Commands.Login;
using HelpDesk.Backend.Application.Features.Auth.Queries.GetCurrentUser;
using HelpDesk.Backend.Application.Tests.Common.TestDoubles;
using HelpDesk.Backend.Domain.Aggregates.Users;
using Moq;

namespace HelpDesk.Backend.Application.Tests.Auth;

public sealed class AuthApplicationTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUserProfile()
    {
        // Arrange
        var category = ApplicationTestData.Category();
        var user = ApplicationTestData.Technician([category.Id]);
        var expectedToken = new AccessTokenResult(
            "access-token",
            ApplicationTestData.Now.AddHours(1));
        var users = new Mock<IUserRepository>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var passwordHasher = new Mock<IPasswordHasher>(MockBehavior.Strict);
        var tokenGenerator = new Mock<IAccessTokenGenerator>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();

        unitOfWork.SetupGet(current => current.Users).Returns(users.Object);
        users
            .Setup(current => current.GetByEmailAsync(user.Email.Value, cancellation.Token))
            .ReturnsAsync(user);
        passwordHasher
            .Setup(current => current.Verify(user.PasswordHash, "secret"))
            .Returns(true);
        tokenGenerator
            .Setup(current => current.Generate(user))
            .Returns(expectedToken);

        var handler = new LoginHandler(
            unitOfWork.Object,
            passwordHasher.Object,
            tokenGenerator.Object,
            new LoginValidator());

        // Act
        var response = await handler.Handle(
            new LoginCommand(user.Email.Value, "secret"),
            cancellation.Token);

        // Assert
        response.AccessToken.Should().Be(expectedToken.AccessToken);
        response.TokenType.Should().Be("Bearer");
        response.ExpiresAtUtc.Should().Be(expectedToken.ExpiresAtUtc);
        response.User.Id.Should().Be(user.Id);
        response.User.TechnicianProfile!.SupportCategoryIds.Should().Contain(category.Id);
        users.Verify(
            current => current.GetByEmailAsync(user.Email.Value, cancellation.Token),
            Times.Once);
        passwordHasher.Verify(
            current => current.Verify(user.PasswordHash, "secret"),
            Times.Once);
        tokenGenerator.Verify(current => current.Generate(It.Is<User>(value => value == user)), Times.Once);
        unitOfWork.VerifyGet(current => current.Users, Times.Once);
        unitOfWork.VerifyNoOtherCalls();
        users.VerifyNoOtherCalls();
        passwordHasher.VerifyNoOtherCalls();
        tokenGenerator.VerifyNoOtherCalls();
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
