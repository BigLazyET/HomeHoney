using HomeHoney.Models;

namespace HomeHoney.Api.Contracts.Documents;

public sealed class UpsertInsuranceDocumentRequest
{
    public Guid Id { get; set; }

    public string PolicyName { get; set; } = string.Empty;

    public Guid InsuredMemberId { get; set; }

    public InsuranceCategory InsuranceCategory { get; set; } = InsuranceCategory.Medical;

    public InsuranceStatus Status { get; set; } = InsuranceStatus.Active;

    public DateOnly EffectiveDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = [];
}
