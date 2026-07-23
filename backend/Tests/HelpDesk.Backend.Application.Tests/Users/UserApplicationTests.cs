using FluentValidation;
using HelpDesk.Backend.Application.Features.Users.Commands.CreateUser;
using HelpDesk.Backend.Application.Features.Users.Commands.UpdateTechnicianProfile;
using HelpDesk.Backend.Application.Features.Users.Queries.GetUserById;
using HelpDesk.Backend.Application.Features.Users.Queries.GetUsers;
using HelpDesk.Backend.Application.Tests.TestDoubles;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Tests.Users;

public sealed class UserApplicationTests
{
    [Fact]
    public async Task CreateUser_HashesPasswordAndPersistsOnce()
    {
        var context = new TestContext();
        var admin = ApplicationTestData.SuperAdmin();
        context.Add(admin);
        using var cancellation = new CancellationTokenSource();
        var handler = new CreateUserHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            context.Clock,
            new CreateUserValidator());
        var command = new CreateUserCommand(
            admin.Id,
            "Nuevo Usuario",
            "nuevo@example.com",
            "secreto",
            UserRole.User,
            [],
            null);

        var userId = await handler.Handle(command, cancellation.Token);

        Assert.Equal(context.Users.AddedUser!.Id, userId);
        Assert.Equal("secreto", context.PasswordHasher.ReceivedPassword);
        Assert.Equal("HASH::secreto", context.Users.AddedUser.PasswordHash);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
        Assert.Equal(cancellation.Token, context.UnitOfWork.ReceivedCancellationToken);
    }

    [Fact]
    public async Task CreateTechnician_BySupervisorOfSameCategory_IsAllowed()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        context.Add(category);
        context.Add(supervisor);
        var handler = new CreateUserHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            context.Clock,
            new CreateUserValidator());

        await handler.Handle(
            new CreateUserCommand(
                supervisor.Id,
                "Técnico Nuevo",
                "tecnico@example.com",
                "password",
                UserRole.Technician,
                [category.Id],
                4),
            CancellationToken.None);

        Assert.Equal(UserRole.Technician, context.Users.AddedUser!.Role);
        Assert.Contains(category.Id, context.Users.AddedUser.TechnicianProfile!.SupportCategoryIds);
    }

    [Fact]
    public async Task CreateTechnician_BySupervisorOfAnotherCategory_IsForbidden()
    {
        var context = new TestContext();
        var ownCategory = ApplicationTestData.Category();
        var otherCategory = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(ownCategory.Id);
        context.Add(ownCategory, otherCategory);
        context.Add(supervisor);
        var handler = new CreateUserHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            context.Clock,
            new CreateUserValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new CreateUserCommand(
                    supervisor.Id,
                    "Técnico Nuevo",
                    "tecnico@example.com",
                    "password",
                    UserRole.Technician,
                    [otherCategory.Id],
                    2),
                CancellationToken.None));

        Assert.Equal(0, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateUser_WithDuplicatedEmail_IsRejected()
    {
        var context = new TestContext();
        var admin = ApplicationTestData.SuperAdmin();
        var existing = ApplicationTestData.User("duplicado@example.com");
        context.Add(admin, existing);
        var handler = new CreateUserHandler(
            context.UnitOfWork,
            context.PasswordHasher,
            context.Clock,
            new CreateUserValidator());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new CreateUserCommand(
                    admin.Id,
                    "Duplicado",
                    "duplicado@example.com",
                    "password",
                    UserRole.User,
                    [],
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateUserValidator_RejectsInvalidTechnicianProfile()
    {
        var validator = new CreateUserValidator();
        var command = new CreateUserCommand(
            Guid.NewGuid(),
            "Técnico",
            "tech@example.com",
            "password",
            UserRole.Technician,
            [],
            null);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "INVALID_USER_PROFILE");
    }

    [Fact]
    public async Task UpdateTechnicianProfile_RequiresSuperAdmin()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        var technician = ApplicationTestData.Technician([category.Id]);
        context.Add(category);
        context.Add(supervisor, technician);
        var handler = new UpdateTechnicianProfileHandler(
            context.UnitOfWork,
            context.Clock,
            new UpdateTechnicianProfileValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new UpdateTechnicianProfileCommand(
                    supervisor.Id,
                    technician.Id,
                    [category.Id],
                    5),
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateTechnicianProfile_ReplacesCategoriesAndCapacity()
    {
        var context = new TestContext();
        var first = ApplicationTestData.Category();
        var second = ApplicationTestData.Category();
        var admin = ApplicationTestData.SuperAdmin();
        var technician = ApplicationTestData.Technician([first.Id], 2);
        context.Add(first, second);
        context.Add(admin, technician);
        var handler = new UpdateTechnicianProfileHandler(
            context.UnitOfWork,
            context.Clock,
            new UpdateTechnicianProfileValidator());

        await handler.Handle(
            new UpdateTechnicianProfileCommand(
                admin.Id,
                technician.Id,
                [second.Id],
                6),
            CancellationToken.None);

        Assert.Equal([second.Id], technician.TechnicianProfile!.SupportCategoryIds);
        Assert.Equal(6, technician.TechnicianProfile.MaxActiveTickets);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task GetUsers_RequiresSuperAdminAndPropagatesCancellation()
    {
        var context = new TestContext();
        var admin = ApplicationTestData.SuperAdmin();
        context.Add(admin);
        var readRepository = new FakeUserReadRepository();
        var handler = new GetUsersHandler(
            context.UnitOfWork,
            readRepository,
            new GetUsersValidator());
        using var cancellation = new CancellationTokenSource();

        await handler.Handle(
            new GetUsersQuery(admin.Id, UserRole.Technician),
            cancellation.Token);

        Assert.Equal(UserRole.Technician, readRepository.ReceivedFilter!.Role);
        Assert.Equal(cancellation.Token, readRepository.ReceivedCancellationToken);
    }

    [Fact]
    public async Task GetUserById_MapsDetailsWithoutPassword()
    {
        var context = new TestContext();
        var admin = ApplicationTestData.SuperAdmin();
        var user = ApplicationTestData.User();
        context.Add(admin, user);
        var handler = new GetUserByIdHandler(
            context.UnitOfWork,
            new GetUserByIdValidator());

        var response = await handler.Handle(
            new GetUserByIdQuery(admin.Id, user.Id),
            CancellationToken.None);

        Assert.Equal(user.Email.Value, response.Email);
        Assert.DoesNotContain("hash", response.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetUsersValidator_RejectsPageSizeAboveMaximum()
    {
        var validator = new GetUsersValidator();

        var result = await validator.ValidateAsync(
            new GetUsersQuery(Guid.NewGuid(), PageSize: 101));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "INVALID_PAGE_SIZE");
    }
}
