using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Domain.Tests.Users;

public sealed class UserTests
{
    [Fact]
    public void CreateUser_DoesNotAttachTechnicalProfiles()
    {
        var user = User.CreateUser("  Ana Pérez  ", "ANA@EXAMPLE.COM", "hash", TestData.Now);

        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal("Ana Pérez", user.FullName);
        Assert.Equal("ana@example.com", user.Email.Value);
        Assert.Null(user.TechnicianProfile);
        Assert.Null(user.SupervisorProfile);
    }

    [Fact]
    public void CreateTechnician_AllowsMultipleCategoriesAndPositiveCapacity()
    {
        var categories = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var technician = User.CreateTechnician(
            "Técnico",
            "tech@example.com",
            "hash",
            categories,
            5,
            TestData.Now);

        Assert.Equal(UserRole.Technician, technician.Role);
        Assert.Equal(5, technician.TechnicianProfile!.MaxActiveTickets);
        Assert.All(categories, categoryId => Assert.True(technician.SupportsCategory(categoryId)));
    }

    [Fact]
    public void RemoveTechnicianCategory_RejectsRemovingLastCategory()
    {
        var technician = TestData.Technician(new[] { Guid.NewGuid() });

        var exception = Assert.Throws<DomainException>(() =>
            technician.RemoveTechnicianCategory(
                technician.TechnicianProfile!.SupportCategoryIds.Single(),
                TestData.Now.AddMinutes(1)));

        Assert.Equal("TECHNICIAN_REQUIRES_CATEGORY", exception.Code);
    }

    [Fact]
    public void CreateSupervisor_AttachesExactlyOneCategory()
    {
        var categoryId = Guid.NewGuid();

        var supervisor = TestData.Supervisor(categoryId);

        Assert.Equal(UserRole.Supervisor, supervisor.Role);
        Assert.Equal(categoryId, supervisor.SupervisorProfile!.SupportCategoryId);
        Assert.Null(supervisor.TechnicianProfile);
    }

    [Fact]
    public void InactiveUser_CannotUpdateIdentity()
    {
        var user = TestData.NormalUser();
        user.Deactivate(TestData.Now.AddMinutes(1));

        var exception = Assert.Throws<DomainException>(() =>
            user.UpdateIdentity("Nuevo nombre", "new@example.com", TestData.Now.AddMinutes(2)));

        Assert.Equal("USER_INACTIVE", exception.Code);
    }
}
