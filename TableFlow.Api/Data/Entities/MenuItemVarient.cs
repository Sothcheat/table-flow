namespace TableFlow.Api.Data.Entities
{
    public class MenuItemVarient
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = null!;
        public string VarientName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
