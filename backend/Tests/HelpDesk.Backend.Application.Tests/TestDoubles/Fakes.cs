using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Application.Abstractions.Persistence;
using HelpDesk.Backend.Application.Abstractions.Queries;
using HelpDesk.Backend.Application.Common.Models;
using HelpDesk.Backend.Application.Features.SupportCategories.Models;
using HelpDesk.Backend.Application.Features.Tickets.Models;
using HelpDesk.Backend.Application.Features.Users.Models;
using HelpDesk.Backend.Domain.Categories;
using HelpDesk.Backend.Domain.Tickets;
using HelpDesk.Backend.Domain.Users;

namespace HelpDesk.Backend.Application.Tests.TestDoubles;

internal sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string? ReceivedPassword { get; private set; }
    public string? ReceivedHash { get; private set; }
    public bool VerificationResult { get; set; } = true;

    public string Hash(string password)
    {
        ReceivedPassword = password;
        return $"HASH::{password}";
    }

    public bool Verify(string passwordHash, string password)
    {
        ReceivedHash = passwordHash;
        ReceivedPassword = password;
        return VerificationResult;
    }
}

internal sealed class FakeAccessTokenGenerator : IAccessTokenGenerator
{
    internal User? ReceivedUser { get; private set; }
    internal AccessTokenResult Result { get; set; } =
        new("access-token", ApplicationTestData.Now.AddHours(1));

    public AccessTokenResult Generate(User user)
    {
        ReceivedUser = user;
        return Result;
    }
}

internal sealed class FakeTicketNumberSequence(int next = 1) : ITicketNumberSequence
{
    public int Next { get; set; } = next;
    public int? ReceivedYear { get; private set; }
    public CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<int> GetNextAsync(int year, CancellationToken cancellationToken)
    {
        ReceivedYear = year;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(Next);
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    internal Dictionary<Guid, User> Items { get; } = [];
    internal User? AddedUser { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        Items.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        var user = Items.Values.SingleOrDefault(candidate =>
            string.Equals(candidate.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<bool> ExistsByEmailAsync(
        string email,
        Guid? excludingUserId,
        CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        var exists = Items.Values.Any(user =>
            user.Id != excludingUserId &&
            string.Equals(user.Email.Value, email.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        AddedUser = user;
        Items[user.Id] = user;
        return Task.CompletedTask;
    }
}

internal sealed class FakeSupportCategoryRepository : ISupportCategoryRepository
{
    internal Dictionary<Guid, SupportCategory> Items { get; } = [];
    internal SupportCategory? AddedCategory { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<SupportCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        Items.TryGetValue(id, out var category);
        return Task.FromResult(category);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludingCategoryId,
        CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        var exists = Items.Values.Any(category =>
            category.Id != excludingCategoryId &&
            string.Equals(category.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(SupportCategory category, CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        AddedCategory = category;
        Items[category.Id] = category;
        return Task.CompletedTask;
    }
}

internal sealed class FakeTicketRepository : ITicketRepository
{
    internal Dictionary<Guid, Ticket> Items { get; } = [];
    internal Dictionary<Guid, int> ActiveCountByTechnician { get; } = [];
    internal List<Ticket> PendingSlaTickets { get; } = [];
    internal List<Ticket> ResolvedForClosure { get; } = [];
    internal Ticket? AddedTicket { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        Items.TryGetValue(id, out var ticket);
        return Task.FromResult(ticket);
    }

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        AddedTicket = ticket;
        Items[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    public Task<int> CountActiveByTechnicianAsync(
        Guid technicianUserId,
        CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(
            ActiveCountByTechnician.GetValueOrDefault(technicianUserId));
    }

    public Task<IReadOnlyList<Ticket>> GetPendingSlaTicketsAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult<IReadOnlyList<Ticket>>(
            PendingSlaTickets.Take(batchSize).ToArray());
    }

    public Task<IReadOnlyList<Ticket>> GetResolvedForAutomaticClosureAsync(
        DateTimeOffset now,
        int batchSize,
        CancellationToken cancellationToken)
    {
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult<IReadOnlyList<Ticket>>(
            ResolvedForClosure.Take(batchSize).ToArray());
    }
}

internal sealed class FakeUnitOfWork(
    FakeUserRepository users,
    FakeSupportCategoryRepository supportCategories,
    FakeTicketRepository tickets,
    FakeTicketNumberSequence ticketNumbers) : IUnitOfWork
{
    public IUserRepository Users => users;
    public ISupportCategoryRepository SupportCategories => supportCategories;
    public ITicketRepository Tickets => tickets;
    public ITicketNumberSequence TicketNumbers => ticketNumbers;
    internal int SaveCount { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(1);
    }
}

internal sealed class TestContext
{
    internal FakeUserRepository Users { get; } = new();
    internal FakeSupportCategoryRepository Categories { get; } = new();
    internal FakeTicketRepository Tickets { get; } = new();
    internal FakeTicketNumberSequence Sequence { get; } = new();
    internal FakeClock Clock { get; } = new(ApplicationTestData.Now);
    internal FakePasswordHasher PasswordHasher { get; } = new();
    internal FakeUnitOfWork UnitOfWork { get; }

    internal TestContext()
    {
        UnitOfWork = new FakeUnitOfWork(Users, Categories, Tickets, Sequence);
    }

    internal void Add(params User[] users)
    {
        foreach (var user in users)
        {
            Users.Items[user.Id] = user;
        }
    }

    internal void Add(params SupportCategory[] categories)
    {
        foreach (var category in categories)
        {
            Categories.Items[category.Id] = category;
        }
    }

    internal void Add(params Ticket[] tickets)
    {
        foreach (var ticket in tickets)
        {
            Tickets.Items[ticket.Id] = ticket;
        }
    }
}

internal sealed class FakeUserReadRepository : IUserReadRepository
{
    internal PagedResponse<UserSummaryResponse> Response { get; set; } =
        new([], 1, 20, 0);
    internal UserReadFilter? ReceivedFilter { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<PagedResponse<UserSummaryResponse>> GetPagedAsync(
        UserReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ReceivedFilter = filter;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(Response);
    }
}

internal sealed class FakeSupportCategoryReadRepository : ISupportCategoryReadRepository
{
    internal PagedResponse<SupportCategorySummaryResponse> Response { get; set; } =
        new([], 1, 20, 0);
    internal SupportCategoryReadFilter? ReceivedFilter { get; private set; }

    public Task<PagedResponse<SupportCategorySummaryResponse>> GetPagedAsync(
        SupportCategoryReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ReceivedFilter = filter;
        return Task.FromResult(Response);
    }
}

internal sealed class FakeTicketReadRepository : ITicketReadRepository
{
    internal PagedResponse<TicketSummaryResponse> TicketResponse { get; set; } =
        new([], 1, 20, 0);
    internal IReadOnlyList<AssignableTechnicianResponse> TechnicianResponse { get; set; } = [];
    internal PagedResponse<SlaAlertResponse> AlertResponse { get; set; } =
        new([], 1, 20, 0);
    internal TicketReadFilter? ReceivedTicketFilter { get; private set; }
    internal TicketVisibilityScope? ReceivedVisibility { get; private set; }
    internal Guid? ReceivedCategoryId { get; private set; }
    internal DateTimeOffset? ReceivedAsOfUtc { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<PagedResponse<TicketSummaryResponse>> GetPagedAsync(
        TicketReadFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ReceivedTicketFilter = filter;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(TicketResponse);
    }

    public Task<IReadOnlyList<AssignableTechnicianResponse>> GetAssignableTechniciansAsync(
        Guid supportCategoryId,
        CancellationToken cancellationToken)
    {
        ReceivedCategoryId = supportCategoryId;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(TechnicianResponse);
    }

    public Task<PagedResponse<SlaAlertResponse>> GetSlaAlertsAsync(
        TicketVisibilityScope visibility,
        Guid? supportCategoryId,
        DateTimeOffset asOfUtc,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ReceivedVisibility = visibility;
        ReceivedCategoryId = supportCategoryId;
        ReceivedAsOfUtc = asOfUtc;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(AlertResponse);
    }
}

internal sealed class FakeSlaReportReadRepository : ISlaReportReadRepository
{
    internal SlaReportResponse Response { get; set; } =
        new([], 0, 0, 0, 0, null);
    internal SlaReportFilter? ReceivedFilter { get; private set; }
    internal CancellationToken ReceivedCancellationToken { get; private set; }

    public Task<SlaReportResponse> GetReportAsync(
        SlaReportFilter filter,
        CancellationToken cancellationToken)
    {
        ReceivedFilter = filter;
        ReceivedCancellationToken = cancellationToken;
        return Task.FromResult(Response);
    }
}
