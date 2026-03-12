using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeHoney.Models;

namespace HomeHoney.Api.Tests.Integration.Documents;

public sealed class DocumentWriteFlowTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public DocumentWriteFlowTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        TestServiceOverrides.Reset();
    }

    [Fact]
    public async Task Insurance_document_can_be_saved_uploaded_and_downloaded()
    {
        using var client = _factory.CreateClient();

        var record = new InsuranceRecord
        {
            PolicyName = "接口新增保单",
            ProviderName = "测试保险",
            Summary = "用于验证后端写链路",
            InsuredMemberId = Guid.NewGuid(),
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
            ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
        };

        using var saveResponse = await client.PostAsJsonAsync("/api/v1/documents/insurance", record);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);

        var savedEnvelope = await saveResponse.Content.ReadFromJsonAsync<ApiEnvelope<InsuranceRecord>>();
        Assert.NotNull(savedEnvelope);
        Assert.True(savedEnvelope.Result.IsSuccess);
        Assert.NotEqual(Guid.Empty, savedEnvelope.Data.Id);

        using var form = new MultipartFormDataContent();
        using var content = new ByteArrayContent([1, 2, 3, 4]);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(content, "file", "policy.pdf");

        using var uploadResponse = await client.PostAsync($"/api/v1/documents/insurance/{savedEnvelope.Data.Id}/files:upload", form);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        var detail = await client.GetFromJsonAsync<InsuranceRecord>($"/api/v1/documents/insurance/{savedEnvelope.Data.Id}");
        Assert.NotNull(detail?.PrimaryFile);

        using var downloadResponse = await client.GetAsync($"/api/v1/documents/insurance/{savedEnvelope.Data.Id}/files/primary:download");
        Assert.Equal(HttpStatusCode.OK, downloadResponse.StatusCode);
        var bytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    private sealed class ApiEnvelope<T>
    {
        public required OperationResult Result { get; set; }
        public required T Data { get; set; }
    }

    private sealed class OperationResult
    {
        public bool IsSuccess { get; set; }
    }
}