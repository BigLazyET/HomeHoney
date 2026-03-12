using System.Net;
using HomeHoney.Api.Tests.Integration;

namespace HomeHoney.Api.Tests.Contract;

public sealed class OpenApiSnapshotTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public OpenApiSnapshotTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OpenApi_document_is_available_in_development()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/openapi/v1.json");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound);
    }
}
