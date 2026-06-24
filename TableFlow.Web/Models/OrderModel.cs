namespace TableFlow.Web.Models;

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

public class SessionStatsModel
{
    public decimal TodayRevenue { get; set; }
    public int TodayClosedSessions { get; set; }
    public int OpenSessions { get; set; }
    public List<TopSellingItemModel> TopItems { get; set; } = new();
}

public class TopSellingItemModel
{
    public string ItemName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
}
