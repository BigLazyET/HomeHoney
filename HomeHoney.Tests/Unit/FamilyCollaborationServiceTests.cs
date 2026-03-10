using HomeHoney.Services.Collaboration;

namespace HomeHoney.Tests.Unit;

public class FamilyCollaborationServiceTests
{
    private readonly FamilyCollaborationService _sut = new();

    [Fact]
    public async Task DeleteFridgeNoteAsync_RemovesExistingNote()
    {
        // Arrange
        var existing = (await _sut.GetFridgeNotesAsync()).First();

        // Act
        var deleted = await _sut.DeleteFridgeNoteAsync(existing.Id);
        var remaining = await _sut.GetFridgeNotesAsync();

        // Assert
        Assert.True(deleted);
        Assert.DoesNotContain(remaining, item => item.Id == existing.Id);
    }
}
