namespace HelpDesk.Backend.Api.Tests.TestSupport;

public abstract class ApiIntegrationTestBase : IAsyncLifetime
{
    private readonly bool _includeBootstrapConfiguration;

    protected ApiIntegrationTestBase(bool includeBootstrapConfiguration = true)
    {
        _includeBootstrapConfiguration = includeBootstrapConfiguration;
    }

    internal HelpDeskApiFactory Factory { get; private set; } = null!;
    internal HttpClient Client { get; private set; } = null!;

    public Task InitializeAsync()
    {
        Factory = new HelpDeskApiFactory(_includeBootstrapConfiguration);
        Client = Factory.CreateApiClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        await Factory.DeleteDatabaseAsync();
    }
}
