namespace HomeHoney.Models;

public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
}

public sealed record ValidationIssue(
    string FieldKey,
    string Message,
    ValidationSeverity Severity = ValidationSeverity.Error,
    string? Code = null,
    bool IsBlocking = true);
