using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.Users;

public sealed class TechnicianCategory
{
    private TechnicianCategory()
    {
    }

    private TechnicianCategory(Guid supportCategoryId)
    {
        SupportCategoryId = Guard.Required(
            supportCategoryId,
            "SUPPORT_CATEGORY_REQUIRED",
            "La categoría de soporte es obligatoria.");
    }

    public Guid SupportCategoryId { get; private set; }

    internal static TechnicianCategory Create(Guid supportCategoryId) =>
        new(supportCategoryId);
}
