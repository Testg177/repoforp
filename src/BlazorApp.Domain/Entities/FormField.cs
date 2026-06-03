using System.Text.Json;
using BlazorApp.Domain.Enums;

namespace BlazorApp.Domain.Entities;

public class FormField
{
    public int Id { get; set; }
    public int FormDefinitionId { get; set; }
    public string FieldKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public string? HelpText { get; set; }
    public FieldType Type { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public Dictionary<string, JsonElement>? ValidationRules { get; set; }
    public List<string>? Options { get; set; }
    public Dictionary<string, JsonElement>? ConditionalLogic { get; set; }

    public FormDefinition FormDefinition { get; set; } = null!;
}
