using HelpDesk.Backend.Domain.Categories;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Tests.Categories;

public sealed class SupportCategoryTests
{
    [Fact]
    public void Create_RequiresSlaForEveryPriority()
    {
        var incomplete = new Dictionary<TicketPriority, TimeSpan>
        {
            [TicketPriority.Low] = TimeSpan.FromHours(24)
        };

        var exception = Assert.Throws<DomainException>(() =>
            SupportCategory.Create("Hardware", "Equipos", incomplete, TestData.Now));

        Assert.Equal("INCOMPLETE_SLA_CONFIGURATION", exception.Code);
    }

    [Fact]
    public void UpdateSla_ChangesOnlySelectedPriority()
    {
        var category = TestData.Category();

        category.UpdateSla(TicketPriority.Critical, TimeSpan.FromHours(1), TestData.Now.AddMinutes(1));

        Assert.Equal(TimeSpan.FromHours(1), category.GetSlaDuration(TicketPriority.Critical));
        Assert.Equal(TimeSpan.FromHours(24), category.GetSlaDuration(TicketPriority.Low));
    }

    [Fact]
    public void Deactivate_PreservesPoliciesButPreventsUsingCategoryForNewSla()
    {
        var category = TestData.Category();

        category.Deactivate(TestData.Now.AddMinutes(1));

        Assert.False(category.IsActive);
        Assert.Equal(4, category.SlaPolicies.Count);
        Assert.Throws<DomainException>(() => category.GetSlaDuration(TicketPriority.High));
    }
}
