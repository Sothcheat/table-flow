namespace TableFlow.Api.Data.Entities
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string ItemName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool HasVarient { get; set; } = false;
        public bool IsAvailable { get; set; }  = true;
        public string? ImageUrl { get; set; }
        public List<MenuItemVarient> MenuItemVarients { get; set; } = new();
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
