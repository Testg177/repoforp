namespace BlazorApp.Domain.Entities;

public class ArchiveCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ArchiveCategory? ParentCategory { get; set; }
    public ICollection<ArchiveCategory> SubCategories { get; set; } = [];
    public ICollection<ArchiveDocument> Documents { get; set; } = [];
}
