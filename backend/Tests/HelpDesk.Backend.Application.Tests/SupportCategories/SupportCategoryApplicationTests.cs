using HelpDesk.Backend.Application.Features.SupportCategories.Commands.CreateSupportCategory;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.SetSupportCategoryActive;
using HelpDesk.Backend.Application.Features.SupportCategories.Commands.UpdateSupportCategorySla;
using HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategories;
using HelpDesk.Backend.Application.Features.SupportCategories.Queries.GetSupportCategoryById;
using HelpDesk.Backend.Application.Tests.Common.TestDoubles;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Application.Tests.SupportCategories;

public sealed class SupportCategoryApplicationTests
{
    [Fact]
    public async Task CreateCategory_CreatesAllSlaPoliciesAndPersistsOnce()
    {
        var context = new TestContext();
        var admin = ApplicationTestData.SuperAdmin();
        context.Add(admin);
        var handler = new CreateSupportCategoryHandler(
            context.UnitOfWork,
            context.Clock,
            new CreateSupportCategoryValidator());

        var categoryId = await handler.Handle(
            new CreateSupportCategoryCommand(
                admin.Id,
                "Software",
                "Soporte de aplicaciones",
                TimeSpan.FromHours(24),
                TimeSpan.FromHours(12),
                TimeSpan.FromHours(6),
                TimeSpan.FromHours(2)),
            CancellationToken.None);

        Assert.Equal(context.Categories.AddedCategory!.Id, categoryId);
        Assert.Equal(4, context.Categories.AddedCategory.SlaPolicies.Count);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicatedName_IsRejected()
    {
        var context = new TestContext();
        var admin = ApplicationTestData.SuperAdmin();
        var category = ApplicationTestData.Category("Software");
        context.Add(admin);
        context.Add(category);
        var handler = new CreateSupportCategoryHandler(
            context.UnitOfWork,
            context.Clock,
            new CreateSupportCategoryValidator());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(
                new CreateSupportCategoryCommand(
                    admin.Id,
                    "Software",
                    "Otra descripción",
                    TimeSpan.FromHours(24),
                    TimeSpan.FromHours(12),
                    TimeSpan.FromHours(6),
                    TimeSpan.FromHours(2)),
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateSla_BySupervisorOfCategory_IsAllowed()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        context.Add(category);
        context.Add(supervisor);
        var handler = new UpdateSupportCategorySlaHandler(
            context.UnitOfWork,
            context.Clock,
            new UpdateSupportCategorySlaValidator());

        await handler.Handle(
            new UpdateSupportCategorySlaCommand(
                supervisor.Id,
                category.Id,
                TicketPriority.High,
                TimeSpan.FromHours(3)),
            CancellationToken.None);

        Assert.Equal(TimeSpan.FromHours(3), category.GetSlaDuration(TicketPriority.High));
    }

    [Fact]
    public async Task UpdateSla_BySupervisorOfAnotherCategory_IsForbidden()
    {
        var context = new TestContext();
        var ownCategory = ApplicationTestData.Category();
        var otherCategory = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(ownCategory.Id);
        context.Add(ownCategory, otherCategory);
        context.Add(supervisor);
        var handler = new UpdateSupportCategorySlaHandler(
            context.UnitOfWork,
            context.Clock,
            new UpdateSupportCategorySlaValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new UpdateSupportCategorySlaCommand(
                    supervisor.Id,
                    otherCategory.Id,
                    TicketPriority.High,
                    TimeSpan.FromHours(3)),
                CancellationToken.None));
    }

    [Fact]
    public async Task SetCategoryActive_RequiresSuperAdmin()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var supervisor = ApplicationTestData.Supervisor(category.Id);
        context.Add(category);
        context.Add(supervisor);
        var handler = new SetSupportCategoryActiveHandler(
            context.UnitOfWork,
            context.Clock,
            new SetSupportCategoryActiveValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new SetSupportCategoryActiveCommand(
                    supervisor.Id,
                    category.Id,
                    false),
                CancellationToken.None));
    }

    [Fact]
    public async Task SetCategoryActive_DeactivatesAndReactivates()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        var admin = ApplicationTestData.SuperAdmin();
        context.Add(category);
        context.Add(admin);
        var handler = new SetSupportCategoryActiveHandler(
            context.UnitOfWork,
            context.Clock,
            new SetSupportCategoryActiveValidator());

        await handler.Handle(
            new SetSupportCategoryActiveCommand(admin.Id, category.Id, false),
            CancellationToken.None);
        context.Clock.UtcNow = context.Clock.UtcNow.AddMinutes(1);
        await handler.Handle(
            new SetSupportCategoryActiveCommand(admin.Id, category.Id, true),
            CancellationToken.None);

        Assert.True(category.IsActive);
        Assert.Equal(2, context.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task GetCategories_NonAdminCannotIncludeInactive()
    {
        var context = new TestContext();
        var user = ApplicationTestData.User();
        context.Add(user);
        var handler = new GetSupportCategoriesHandler(
            context.UnitOfWork,
            new FakeSupportCategoryReadRepository(),
            new GetSupportCategoriesValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetSupportCategoriesQuery(user.Id, IncludeInactive: true),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetInactiveCategory_NonAdminIsForbidden()
    {
        var context = new TestContext();
        var category = ApplicationTestData.Category();
        category.Deactivate(ApplicationTestData.Now.AddMinutes(1));
        var user = ApplicationTestData.User();
        context.Add(category);
        context.Add(user);
        var handler = new GetSupportCategoryByIdHandler(
            context.UnitOfWork,
            new GetSupportCategoryByIdValidator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(
                new GetSupportCategoryByIdQuery(user.Id, category.Id),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateCategoryValidator_RejectsNonPositiveSla()
    {
        var validator = new CreateSupportCategoryValidator();

        var result = await validator.ValidateAsync(
            new CreateSupportCategoryCommand(
                Guid.NewGuid(),
                "Software",
                "Descripción",
                TimeSpan.Zero,
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(1),
                TimeSpan.FromHours(1)));

        Assert.False(result.IsValid);
    }
}
