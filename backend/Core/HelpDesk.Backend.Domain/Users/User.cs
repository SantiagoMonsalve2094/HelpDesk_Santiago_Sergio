using HelpDesk.Backend.Domain.Common;
using HelpDesk.Backend.Domain.Enums;
using HelpDesk.Backend.Domain.ValueObjects;

namespace HelpDesk.Backend.Domain.Users;

public sealed class User : AggregateRoot
{
    private const int FullNameMaxLength = 200;
    private const int PasswordHashMaxLength = 500;

    private User(
        Guid id,
        string fullName,
        EmailAddress email,
        string passwordHash,
        UserRole role,
        TechnicianProfile? technicianProfile,
        SupervisorProfile? supervisorProfile,
        DateTimeOffset now)
        : base(id)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        TechnicianProfile = technicianProfile;
        SupervisorProfile = supervisorProfile;
        IsActive = true;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public string FullName { get; private set; }
    public EmailAddress Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; }
    public bool IsActive { get; private set; }
    public TechnicianProfile? TechnicianProfile { get; }
    public SupervisorProfile? SupervisorProfile { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static User CreateUser(string fullName, string email, string passwordHash, DateTimeOffset now) =>
        Create(fullName, email, passwordHash, UserRole.User, null, null, now);

    public static User CreateSuperAdmin(string fullName, string email, string passwordHash, DateTimeOffset now) =>
        Create(fullName, email, passwordHash, UserRole.SuperAdmin, null, null, now);

    public static User CreateTechnician(
        string fullName,
        string email,
        string passwordHash,
        IEnumerable<Guid> supportCategoryIds,
        int maxActiveTickets,
        DateTimeOffset now) =>
        Create(
            fullName,
            email,
            passwordHash,
            UserRole.Technician,
            TechnicianProfile.Create(supportCategoryIds, maxActiveTickets),
            null,
            now);

    public static User CreateSupervisor(
        string fullName,
        string email,
        string passwordHash,
        Guid supportCategoryId,
        DateTimeOffset now) =>
        Create(
            fullName,
            email,
            passwordHash,
            UserRole.Supervisor,
            null,
            SupervisorProfile.Create(supportCategoryId),
            now);

    public void UpdateIdentity(string fullName, string email, DateTimeOffset now)
    {
        EnsureActive();
        FullName = NormalizeFullName(fullName);
        Email = EmailAddress.Create(email);
        UpdatedAtUtc = now;
    }

    public void ChangePasswordHash(string passwordHash, DateTimeOffset now)
    {
        EnsureActive();
        PasswordHash = NormalizePasswordHash(passwordHash);
        UpdatedAtUtc = now;
    }

    public void AddTechnicianCategory(Guid supportCategoryId, DateTimeOffset now)
    {
        EnsureActive();
        EnsureTechnician().AddCategory(supportCategoryId);
        UpdatedAtUtc = now;
    }

    public void RemoveTechnicianCategory(Guid supportCategoryId, DateTimeOffset now)
    {
        EnsureActive();
        EnsureTechnician().RemoveCategory(supportCategoryId);
        UpdatedAtUtc = now;
    }

    public void ChangeTechnicianCapacity(int maxActiveTickets, DateTimeOffset now)
    {
        EnsureActive();
        EnsureTechnician().ChangeCapacity(maxActiveTickets);
        UpdatedAtUtc = now;
    }

    public void ChangeSupervisorCategory(Guid supportCategoryId, DateTimeOffset now)
    {
        EnsureActive();
        if (SupervisorProfile is null)
        {
            throw new DomainException("USER_IS_NOT_SUPERVISOR", "El usuario no tiene perfil de supervisor.");
        }

        SupervisorProfile.ChangeCategory(supportCategoryId);
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
            throw new DomainException("USER_ALREADY_ACTIVE", "El usuario ya está activo.");
        }

        IsActive = true;
        UpdatedAtUtc = now;
    }

    public bool SupportsCategory(Guid supportCategoryId) =>
        TechnicianProfile?.Supports(supportCategoryId) == true;

    private static User Create(
        string fullName,
        string email,
        string passwordHash,
        UserRole role,
        TechnicianProfile? technicianProfile,
        SupervisorProfile? supervisorProfile,
        DateTimeOffset now)
    {
        ValidateRoleProfiles(role, technicianProfile, supervisorProfile);
        return new User(
            Guid.NewGuid(),
            NormalizeFullName(fullName),
            EmailAddress.Create(email),
            NormalizePasswordHash(passwordHash),
            role,
            technicianProfile,
            supervisorProfile,
            now);
    }

    private static void ValidateRoleProfiles(
        UserRole role,
        TechnicianProfile? technicianProfile,
        SupervisorProfile? supervisorProfile)
    {
        var valid = role switch
        {
            UserRole.Technician => technicianProfile is not null && supervisorProfile is null,
            UserRole.Supervisor => technicianProfile is null && supervisorProfile is not null,
            UserRole.User or UserRole.SuperAdmin => technicianProfile is null && supervisorProfile is null,
            _ => false
        };

        if (!valid)
        {
            throw new DomainException("INVALID_USER_PROFILE", "El perfil no corresponde al rol del usuario.");
        }
    }

    private static string NormalizeFullName(string fullName) =>
        Guard.Required(fullName, FullNameMaxLength, "INVALID_FULL_NAME", "El nombre completo es obligatorio y admite máximo 200 caracteres.");

    private static string NormalizePasswordHash(string passwordHash) =>
        Guard.Required(passwordHash, PasswordHashMaxLength, "INVALID_PASSWORD_HASH", "El hash de contraseña es obligatorio y admite máximo 500 caracteres.");

    private TechnicianProfile EnsureTechnician() =>
        TechnicianProfile ?? throw new DomainException("USER_IS_NOT_TECHNICIAN", "El usuario no tiene perfil de técnico.");

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainException("USER_INACTIVE", "El usuario está inactivo.");
        }
    }
}
