using HomeHoney.Services.Documents;

namespace HomeHoney.Tests.Unit;

public class DocumentCatalogServiceTests
{
    private readonly DocumentCatalogService _sut = new();

    [Fact]
    public async Task DeleteInsuranceRecordAsync_RemovesExistingRecord()
    {
        // Arrange
        var existing = (await _sut.GetInsuranceRecordsAsync()).First();

        // Act
        var deleted = await _sut.DeleteInsuranceRecordAsync(existing.Id);
        var remaining = await _sut.GetInsuranceRecordsAsync();

        // Assert
        Assert.True(deleted);
        Assert.DoesNotContain(remaining, item => item.Id == existing.Id);
    }

    [Fact]
    public async Task DeleteManualRecordAsync_RemovesExistingRecord()
    {
        // Arrange
        var existing = (await _sut.GetManualRecordsAsync()).First();

        // Act
        var deleted = await _sut.DeleteManualRecordAsync(existing.Id);
        var remaining = await _sut.GetManualRecordsAsync();

        // Assert
        Assert.True(deleted);
        Assert.DoesNotContain(remaining, item => item.Id == existing.Id);
    }
}
