using System.Collections.ObjectModel;
using HelpDesk.Backend.Domain.Common;

namespace HelpDesk.Backend.Domain.Aggregates.Users;

public sealed class TechnicianProfile
{
    private readonly List<TechnicianCategory> _categoryAssignments = [];

    private TechnicianProfile(IEnumerable<Guid> supportCategoryIds, int maxActiveTickets)
    {
        var categoryIds = supportCategoryIds.Distinct().ToArray();
        EnsureValidCategories(categoryIds);
        _categoryAssignments.AddRange(categoryIds.Select(TechnicianCategory.Create));
        MaxActiveTickets = EnsureValidCapacity(maxActiveTickets);
    }

    private TechnicianProfile()
    {
    }

    public IReadOnlyCollection<Guid> SupportCategoryIds =>
        new ReadOnlyCollection<Guid>(
            _categoryAssignments
                .Select(assignment => assignment.SupportCategoryId)
                .Order()
                .ToList());

    public IReadOnlyCollection<TechnicianCategory> CategoryAssignments =>
        new ReadOnlyCollection<TechnicianCategory>(_categoryAssignments);

    public int MaxActiveTickets { get; private set; }

    public static TechnicianProfile Create(IEnumerable<Guid> supportCategoryIds, int maxActiveTickets)
    {
        ArgumentNullException.ThrowIfNull(supportCategoryIds);
        return new TechnicianProfile(supportCategoryIds, maxActiveTickets);
    }

    public bool Supports(Guid supportCategoryId) =>
        _categoryAssignments.Any(assignment => assignment.SupportCategoryId == supportCategoryId);

    internal void AddCategory(Guid supportCategoryId)
    {
        Guard.Required(
            supportCategoryId,
            "SUPPORT_CATEGORY_REQUIRED",
            "La categoría de soporte es obligatoria.");

        if (!Supports(supportCategoryId))
        {
            _categoryAssignments.Add(TechnicianCategory.Create(supportCategoryId));
        }
    }

    internal void RemoveCategory(Guid supportCategoryId)
    {
        var assignment = _categoryAssignments
            .SingleOrDefault(item => item.SupportCategoryId == supportCategoryId);
        if (assignment is null)
        {
            throw new DomainException("TECHNICIAN_CATEGORY_NOT_FOUND", "El técnico no está habilitado para la categoría indicada.");
        }

        if (_categoryAssignments.Count == 1)
        {
            throw new DomainException("TECHNICIAN_REQUIRES_CATEGORY", "Un técnico debe conservar al menos una categoría.");
        }

        _categoryAssignments.Remove(assignment);
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
