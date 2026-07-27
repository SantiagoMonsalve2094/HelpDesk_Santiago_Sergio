using HelpDesk.Backend.Domain.Aggregates.SupportCategories;
using HelpDesk.Backend.Domain.Aggregates.Tickets;
using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Domain.Entities.Tickets;
using HelpDesk.Backend.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Backend.Infrastructure.Persistence;

public sealed class HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<SupportCategory> SupportCategories => Set<SupportCategory>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    internal DbSet<TicketSlaCycle> TicketSlaCycles => Set<TicketSlaCycle>();
    internal DbSet<TicketNumberSequenceState> TicketNumberSequences =>
        Set<TicketNumberSequenceState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HelpDeskDbContext).Assembly);
    }
}
