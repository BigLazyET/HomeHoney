using HomeHoney.Models;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Documents;
using HomeHoney.Services.Reminders;
using HomeHoney.Services.Storage;
using Moq;

namespace HomeHoney.Tests.Unit.Reminders;

public sealed class ReminderCenterServiceApiTests
{
    [Fact]
    public async Task GetUpcomingAsync_uses_backend_api_when_profile_url_is_available_even_if_not_prevalidated()
    {
        var profileService = new Mock<IStorageConnectionProfileService>();
        profileService.Setup(x => x.GetActiveProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageConnectionProfile
            {
                ApiBaseUrl = StorageConnectionProfile.DefaultBackendApiBaseUrl,
                IsActive = true,
                ValidationStatus = StorageValidationStatus.Unknown,
            });

        var apiClient = new Mock<IReminderApiClient>();
        apiClient.Setup(x => x.GetUpcomingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new ReminderItem
                {
                    SourceType = ReminderSourceType.FridgeNote,
                    SourceId = Guid.NewGuid(),
                    Title = "接娃提醒",
                    Summary = "周三 17:20 接孩子",
                    DueAt = DateTime.Today.AddDays(1),
                }
            ]);

        var sut = new ReminderCenterService(new DocumentCatalogService(), new FamilyCollaborationService(), apiClient.Object, profileService.Object);

        var items = await sut.GetUpcomingAsync();

        Assert.Single(items);
        apiClient.Verify(x => x.GetUpcomingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
