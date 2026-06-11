namespace TableFlow.Web.Models;

// ── Response models (mirror API DTOs) ────────────────────────

public record CategoryModel
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public int ItemCount { get; set; }
}

public class MenuItemModel
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsAvailable { get; set; }
    public bool HasVarient { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<VarientModel> Varients { get; set; } = new();
}

public class VarientModel
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string VarientName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

// ── Form models (what dialogs bind to) ───────────────────────

public class CategoryFormModel
{
    public string CategoryName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class MenuItemFormModel
{
    public string ItemName { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public bool IsAvailable { get; set; } = true;   // default true — new items are available
    public bool HasVarient { get; set; } = false;
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
}

public class VarientFormModel
{
    public string VarientName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}