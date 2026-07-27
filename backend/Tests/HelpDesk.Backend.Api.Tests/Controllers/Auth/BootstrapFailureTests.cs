using HelpDesk.Backend.Api.Tests.Common;

namespace HelpDesk.Backend.Api.Tests;

public sealed class BootstrapFailureTests
{
    [Fact]
    public async Task EmptyDatabaseWithoutBootstrapConfiguration_FailsAtStartup()
    {
        var factory = new HelpDeskApiFactory(includeBootstrapConfiguration: false);

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateApiClient());

        Assert.Contains(
            "BootstrapAdmin__FullName",
            exception.ToString(),
            StringComparison.Ordinal);
        factory.Dispose();
        await factory.DeleteDatabaseAsync();
    }
}
