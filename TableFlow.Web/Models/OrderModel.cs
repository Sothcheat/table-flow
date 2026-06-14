namespace TableFlow.Web.Models;

public class SessionModel
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int TableNumber { get; set; }
    public string SessionStatus { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
}

public class OrderModel
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Note { get; set; }
    public List<OrderItemModel> Items { get; set; } = new();
    public decimal OrderTotal { get; set; }
}

public class OrderItemModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int? VarientId { get; set; }
    public string? VarientName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string OrderItemStatus { get; set; } = string.Empty;
    public string? Note { get; set; }
}