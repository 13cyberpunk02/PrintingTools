namespace PrintingTools.Domain.Common;

public interface IAuditableEntity
{
    string? CreatedBy { get; }
    string? UpdatedBy { get; }
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}