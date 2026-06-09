namespace TableFlow.Api.Data.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public TableSession TableSession { get; set; } = null!;

        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
        public string? Note { get; set; }

        // One order has many order items
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
