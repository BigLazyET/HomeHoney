using HomeHoney.Models;

namespace HomeHoney.Api.Contracts.Documents;

public sealed class UpsertManualDocumentRequest
{
    public Guid Id { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public Guid SpaceId { get; set; }

    public ManualCategory ManualCategory { get; set; } = ManualCategory.Appliance;

    public DateOnly PurchaseDate { get; set; }

    public DateOnly WarrantyExpiryDate { get; set; }

    public string Summary { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];
}
