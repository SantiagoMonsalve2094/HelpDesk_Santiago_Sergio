using System.Collections.ObjectModel;
using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;

namespace HelpDesk.Backend.Domain.Categories;

public sealed class SupportCategory : AggregateRoot
{
    private const int NameMaxLength = 100;
    private const int DescriptionMaxLength = 1000;
    private readonly List<SlaPolicy> _slaPolicies = [];

    private SupportCategory()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    private SupportCategory(
        Guid id,
        string name,
        string description,
        IEnumerable<SlaPolicy> slaPolicies,
        DateTimeOffset now)
        : base(id)
    {
        Name = name;
        Description = description;
        _slaPolicies.AddRange(slaPolicies);
        IsActive = true;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<SlaPolicy> SlaPolicies =>
        new ReadOnlyCollection<SlaPolicy>(_slaPolicies);

    public static SupportCategory Create(
        string name,
        string description,
        IReadOnlyDictionary<TicketPriority, TimeSpan> slaDurations,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(slaDurations);
        EnsureCompleteSla(slaDurations);

        var policies = slaDurations
            .OrderBy(item => item.Key)
            .Select(item => new SlaPolicy(Guid.NewGuid(), item.Key, item.Value));

        return new SupportCategory(
            Guid.NewGuid(),
            NormalizeName(name),
            NormalizeDescription(description),
            policies,
            now);
    }

    public TimeSpan GetSlaDuration(TicketPriority priority)
    {
        EnsureActive();
        return _slaPolicies.Single(policy => policy.Priority == priority).ResponseTime;
    }

    public void UpdateInformation(string name, string description, DateTimeOffset now)
    {
        EnsureActive();
        Name = NormalizeName(name);
        Description = NormalizeDescription(description);
        UpdatedAtUtc = now;
    }

    public void UpdateSla(TicketPriority priority, TimeSpan responseTime, DateTimeOffset now)
    {
        EnsureActive();
        _slaPolicies.Single(policy => policy.Priority == priority).Update(responseTime);
        UpdatedAtUtc = now;
    }

    public void Deactivate(DateTimeOffset now)
    {
        EnsureActive();
        IsActive = false;
        UpdatedAtUtc = now;
    }

    public void Activate(DateTimeOffset now)
    {
        if (IsActive)
        {
            throw new DomainException("CATEGORY_ALREADY_ACTIVE", "La categoría ya está activa.");
        }

        IsActive = true;
        UpdatedAtUtc = now;
    }

    private static void EnsureCompleteSla(IReadOnlyDictionary<TicketPriority, TimeSpan> slaDurations)
    {
        var priorities = Enum.GetValues<TicketPriority>();
        if (slaDurations.Count != priorities.Length || priorities.Any(priority => !slaDurations.ContainsKey(priority)))
        {
            throw new DomainException("INCOMPLETE_SLA_CONFIGURATION", "La categoría debe definir SLA para las cuatro prioridades.");
        }

        foreach (var duration in slaDurations.Values)
        {
            Guard.PositiveDuration(duration, "INVALID_SLA_DURATION", "La duración del SLA debe ser mayor que cero.");
        }
    }

    private static string NormalizeName(string name) =>
        Guard.Required(name, NameMaxLength, "INVALID_CATEGORY_NAME", "El nombre de la categoría es obligatorio y admite máximo 100 caracteres.");

    private static string NormalizeDescription(string description) =>
        Guard.Required(description, DescriptionMaxLength, "INVALID_CATEGORY_DESCRIPTION", "La descripción de la categoría es obligatoria y admite máximo 1000 caracteres.");

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainException("CATEGORY_INACTIVE", "La categoría está inactiva.");
        }
    }
}
