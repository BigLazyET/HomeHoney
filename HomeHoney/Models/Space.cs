namespace HomeHoney.Models;

public sealed class Space
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Icon { get; set; } = "🏠";

    public int SortOrder { get; set; }
}
