using System.Collections.ObjectModel;
using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.Users;

public sealed class TechnicianProfile
{
    private readonly HashSet<Guid> _supportCategoryIds;

    private TechnicianProfile(IEnumerable<Guid> supportCategoryIds, int maxActiveTickets)
    {
        _supportCategoryIds = supportCategoryIds.ToHashSet();
        EnsureValidCategories(_supportCategoryIds);
        MaxActiveTickets = EnsureValidCapacity(maxActiveTickets);
    }

    public IReadOnlyCollection<Guid> SupportCategoryIds =>
        new ReadOnlyCollection<Guid>(_supportCategoryIds.Order().ToList());

    public int MaxActiveTickets { get; private set; }

    public static TechnicianProfile Create(IEnumerable<Guid> supportCategoryIds, int maxActiveTickets)
    {
        ArgumentNullException.ThrowIfNull(supportCategoryIds);
        return new TechnicianProfile(supportCategoryIds, maxActiveTickets);
    }

    public bool Supports(Guid supportCategoryId) => _supportCategoryIds.Contains(supportCategoryId);

    internal void AddCategory(Guid supportCategoryId)
    {
        Guard.Required(
            supportCategoryId,
            "SUPPORT_CATEGORY_REQUIRED",
            "La categoría de soporte es obligatoria.");

        _supportCategoryIds.Add(supportCategoryId);
    }

    internal void RemoveCategory(Guid supportCategoryId)
    {
        if (!_supportCategoryIds.Contains(supportCategoryId))
        {
            throw new DomainException("TECHNICIAN_CATEGORY_NOT_FOUND", "El técnico no está habilitado para la categoría indicada.");
        }

        if (_supportCategoryIds.Count == 1)
        {
            throw new DomainException("TECHNICIAN_REQUIRES_CATEGORY", "Un técnico debe conservar al menos una categoría.");
        }

        _supportCategoryIds.Remove(supportCategoryId);
    }

    internal void ChangeCapacity(int maxActiveTickets) =>
        MaxActiveTickets = EnsureValidCapacity(maxActiveTickets);

    private static void EnsureValidCategories(IReadOnlyCollection<Guid> supportCategoryIds)
    {
        if (supportCategoryIds.Count == 0 || supportCategoryIds.Any(id => id == Guid.Empty))
        {
            throw new DomainException("TECHNICIAN_REQUIRES_CATEGORY", "Un técnico debe tener al menos una categoría válida.");
        }
    }

    private static int EnsureValidCapacity(int maxActiveTickets)
    {
        if (maxActiveTickets < 1)
        {
            throw new DomainException("INVALID_TECHNICIAN_CAPACITY", "La capacidad del técnico debe ser mayor que cero.");
        }

        return maxActiveTickets;
    }
}
