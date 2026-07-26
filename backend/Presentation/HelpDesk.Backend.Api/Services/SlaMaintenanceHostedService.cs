using HelpDesk.Backend.Application.Features.Sla.Commands.EvaluatePendingSla;
using HelpDesk.Backend.Application.Features.Tickets.Commands.CloseResolvedTickets;
using MediatR;

namespace HelpDesk.Backend.Api.Services;

internal sealed class SlaMaintenanceHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<SlaMaintenanceHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunMaintenanceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "No se pudo ejecutar el mantenimiento automático de SLA.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunMaintenanceAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new EvaluatePendingSlaCommand(), cancellationToken);
        await sender.Send(new CloseResolvedTicketsCommand(), cancellationToken);
    }
}
