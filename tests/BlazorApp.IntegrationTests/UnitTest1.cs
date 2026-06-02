namespace BlazorApp.IntegrationTests;

public sealed class PostgreSqlIntegrationPlaceholderTests
{
    [Fact(Skip = "Requires Docker runtime and unrestricted build environment.")]
    public void Placeholder()
    {
        Assert.True(true);
    }
}
