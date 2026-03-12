using HomeHoney.Models;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Storage;
using Moq;

namespace HomeHoney.Tests.Unit.Documents;

public sealed class DocumentCatalogServiceApiTests
{
    [Fact]
    public async Task GetInsuranceRecordsAsync_uses_backend_api_when_profile_url_is_available_even_if_not_prevalidated()
    {
        var profileService = new Mock<IStorageConnectionProfileService>();
        profileService.Setup(x => x.GetActiveProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageConnectionProfile
            {
                ApiBaseUrl = StorageConnectionProfile.DefaultBackendApiBaseUrl,
                IsActive = true,
                ValidationStatus = StorageValidationStatus.Unknown,
            });

        var apiClient = new Mock<IDocumentApiClient>();
        apiClient.Setup(x => x.GetInsuranceRecordsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new InsuranceRecord
                {
                    Id = Guid.NewGuid(),
                    PolicyName = "家庭综合险",
                    InsuredMemberId = Guid.NewGuid(),
                    EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                    ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                    ProviderName = "平安保险",
                }
            ]);

        var sut = new DocumentCatalogService(profileService.Object, apiClient.Object);

        var records = await sut.GetInsuranceRecordsAsync();

        Assert.Single(records);
        apiClient.Verify(x => x.GetInsuranceRecordsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
