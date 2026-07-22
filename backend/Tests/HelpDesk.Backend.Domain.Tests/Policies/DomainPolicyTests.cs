using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.Policies;

namespace HelpDesk.Backend.Domain.Tests.Policies;

public sealed class DomainPolicyTests
{
    [Fact]
    public void Supervisor_CanCreateNormalUserAndTechnicianOnlyForOwnCategory()
    {
        var ownCategoryId = Guid.NewGuid();
        var supervisor = TestData.Supervisor(ownCategoryId);

        UserProvisioningPolicy.EnsureCanCreate(supervisor, UserRole.User, Array.Empty<Guid>());
        UserProvisioningPolicy.EnsureCanCreate(supervisor, UserRole.Technician, new[] { ownCategoryId });

        var exception = Assert.Throws<DomainException>(() =>
            UserProvisioningPolicy.EnsureCanCreate(
                supervisor,
                UserRole.Technician,
                new[] { Guid.NewGuid() }));
        Assert.Equal("USER_CREATION_OUTSIDE_SCOPE", exception.Code);
    }

    [Fact]
    public void AssignmentPolicy_RejectsTechnicianFromAnotherCategory()
    {
        var ticketCategoryId = Guid.NewGuid();
        var actor = TestData.Supervisor(ticketCategoryId);
        var technician = TestData.Technician(new[] { Guid.NewGuid() });
        var ticket = TestData.Ticket(TestData.NormalUser().Id, ticketCategoryId);

        var exception = Assert.Throws<DomainException>(() =>
            TicketAssignmentPolicy.EnsureCanAssign(actor, ticket, technician, 0));

        Assert.Equal("TECHNICIAN_NOT_QUALIFIED", exception.Code);
    }

    [Fact]
    public void AssignmentPolicy_RejectsTechnicianAtCapacity()
    {
        var categoryId = Guid.NewGuid();
        var actor = TestData.Supervisor(categoryId);
        var technician = TestData.Technician(new[] { categoryId }, capacity: 2);
        var ticket = TestData.Ticket(TestData.NormalUser().Id, categoryId);

        var exception = Assert.Throws<DomainException>(() =>
            TicketAssignmentPolicy.EnsureCanAssign(actor, ticket, technician, 2));

        Assert.Equal("TECHNICIAN_AT_CAPACITY", exception.Code);
    }

    [Fact]
    public void Visibility_FollowsCreatorTechnicianSupervisorAndAdminRules()
    {
        var categoryId = Guid.NewGuid();
        var creator = TestData.NormalUser();
        var technician = TestData.Technician(new[] { categoryId });
        var supervisor = TestData.Supervisor(categoryId);
        var admin = TestData.SuperAdmin();
        var unrelatedUser = TestData.NormalUser();
        var ticket = TestData.Ticket(creator.Id, categoryId);
        ticket.Assign(technician.Id, supervisor.Id, TestData.Now.AddMinutes(1));

        Assert.True(TicketAccessPolicy.CanView(creator, ticket));
        Assert.True(TicketAccessPolicy.CanView(technician, ticket));
        Assert.True(TicketAccessPolicy.CanView(supervisor, ticket));
        Assert.True(TicketAccessPolicy.CanView(admin, ticket));
        Assert.False(TicketAccessPolicy.CanView(unrelatedUser, ticket));
    }
}
