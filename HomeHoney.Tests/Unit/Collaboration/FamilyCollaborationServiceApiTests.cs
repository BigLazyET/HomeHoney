using HomeHoney.Models;
using HomeHoney.Services.Collaboration;
using HomeHoney.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HomeHoney.Tests.Unit.Collaboration;

public sealed class FamilyCollaborationServiceApiTests
{
    [Fact]
    public async Task SaveFridgeNoteAsync_uses_backend_api_when_profile_url_is_available_even_if_not_prevalidated()
    {
        var profileService = new Mock<IStorageConnectionProfileService>();
        profileService.Setup(x => x.GetActiveProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageConnectionProfile
            {
                ApiBaseUrl = StorageConnectionProfile.DefaultBackendApiBaseUrl,
                IsActive = true,
                ValidationStatus = StorageValidationStatus.Unknown,
            });

        var apiClient = new Mock<ICollaborationApiClient>();
        apiClient.Setup(x => x.SaveFridgeNoteAsync(It.IsAny<FridgeNote>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FridgeNote note, CancellationToken _) =>
            {
                note.Id = note.Id == Guid.Empty ? Guid.NewGuid() : note.Id;
                return note;
            });

        var sut = new FamilyCollaborationService(profileService.Object, apiClient.Object);

        await sut.SaveFridgeNoteAsync(new FridgeNote
        {
            Title = "周末采购",
            Content = "牛奶",
        });

        apiClient.Verify(x => x.SaveFridgeNoteAsync(It.IsAny<FridgeNote>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Service_provider_resolution_uses_backend_enabled_constructor()
    {
        var profileService = new Mock<IStorageConnectionProfileService>();
        profileService.Setup(x => x.GetActiveProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StorageConnectionProfile
            {
                ApiBaseUrl = StorageConnectionProfile.DefaultBackendApiBaseUrl,
                IsActive = true,
                ValidationStatus = StorageValidationStatus.Unknown,
            });

        var apiClient = new Mock<ICollaborationApiClient>();
        apiClient.Setup(x => x.SaveFridgeNoteAsync(It.IsAny<FridgeNote>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FridgeNote note, CancellationToken _) => note);

        var services = new ServiceCollection();
        services.AddSingleton(profileService.Object);
        services.AddSingleton(apiClient.Object);
        services.AddSingleton<FamilyCollaborationService>();

        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<FamilyCollaborationService>();

        await sut.SaveFridgeNoteAsync(new FridgeNote
        {
            Title = "新建冰箱贴",
            Content = "测试后端调用",
        });

        apiClient.Verify(x => x.SaveFridgeNoteAsync(It.IsAny<FridgeNote>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
