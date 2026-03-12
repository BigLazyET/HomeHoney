using HomeHoney.Models;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Search;
using HomeHoney.Services.Storage;
using Moq;

namespace HomeHoney.Tests.Unit.Search;

public sealed class SearchIndexServiceApiTests
{
    [Fact]
    public async Task SearchAsync_uses_backend_api_when_profile_url_is_available_even_if_not_prevalidated()
    {
        var profileService = new Mock<IStorageConnectionProfileService>();
        profileService.Setup(x => x.GetActiveProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageConnectionProfile
            {
                ApiBaseUrl = StorageConnectionProfile.DefaultBackendApiBaseUrl,
                IsActive = true,
                ValidationStatus = StorageValidationStatus.Unknown,
            });

        var apiClient = new Mock<ISearchApiClient>();
        apiClient.Setup(x => x.SearchAsync("采购", It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new SearchGroupResult("冰箱贴", [new SearchResultItem("周末采购", "牛奶、鸡蛋", "冰箱贴", "/fridge-notes")])
            ]);

        var sut = new SearchIndexService(new DocumentCatalogService(), new FamilyCollaborationService(), profileService.Object, apiClient.Object);

        var groups = await sut.SearchAsync("采购");

        Assert.Single(groups);
        apiClient.Verify(x => x.SearchAsync("采购", It.IsAny<CancellationToken>()), Times.Once);
    }
}
