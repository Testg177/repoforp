namespace BlazorApp.Domain.Entities;

public class RejectionCategory
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int FormDefinitionId { get; set; }
    public bool IsActive { get; set; } = true;

    public FormDefinition FormDefinition { get; set; } = null!;
}
