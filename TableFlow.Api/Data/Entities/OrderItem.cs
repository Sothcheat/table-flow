namespace TableFlow.Api.Data.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = null!;
        public int? VarientId { get; set; }
        public MenuItemVarient? Varient { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public OrderItemStatus OrderItemStatus { get; set; } = OrderItemStatus.Waiting;
        public string? Note { get; set; }
    }
}
