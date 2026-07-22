using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.Users;

public sealed class SupervisorProfile
{
    private SupervisorProfile(Guid supportCategoryId)
    {
        SupportCategoryId = Guard.Required(
            supportCategoryId,
            "SUPERVISOR_CATEGORY_REQUIRED",
            "El supervisor debe administrar una categoría.");
    }

    public Guid SupportCategoryId { get; private set; }

    public static SupervisorProfile Create(Guid supportCategoryId) => new(supportCategoryId);

    internal void ChangeCategory(Guid supportCategoryId)
    {
        SupportCategoryId = Guard.Required(
            supportCategoryId,
            "SUPERVISOR_CATEGORY_REQUIRED",
            "El supervisor debe administrar una categoría.");
    }
}
