namespace HomeHoney.Models;

public enum ManualCategory
{
    Appliance,
    Furniture,
    Electronics,
    Kids,
    Other,
}

public sealed class ManualRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string DeviceName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public Guid SpaceId { get; set; }

    public ManualCategory ManualCategory { get; set; } = ManualCategory.Appliance;

    public DateOnly PurchaseDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly WarrantyExpiryDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(2));

    public string Summary { get; set; } = string.Empty;

    public int AttachmentCount { get; set; }

    public FileResource? PrimaryFile { get; set; }

    public string IntegrityStatus { get; set; } = "Healthy";

    public string? SyncMessage { get; set; }

    public List<string> Tags { get; set; } = [];

    public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
}
