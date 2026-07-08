namespace TableFlow.Web.Services
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int? VarientId { get; set; }
        public string? VarientName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;

        // Unique key to identify this cart line
        // Same item + same variant + same note = same line (quantity increases)
        // Same item + different variant or note = different line
        public string Key => $"{MenuItemId}-{VarientId?.ToString() ?? "0"}-{Note ?? ""}";
    }

    public class CartService
    {
        private readonly List<CartItem> _items = new();

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        public int TotalCount => _items.Count;

        public decimal TotalAmount => _items.Sum(i => i.TotalPrice);

        public event Action? OnCartChanged;

        public void AddItem(CartItem item)
        {
            var existing = _items.FirstOrDefault(i => i.Key == item.Key);
            if (existing is not null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                _items.Add(item);
            }
            OnCartChanged?.Invoke();
        }

        public void RemoveItem(string key)
        {
            var item = _items.FirstOrDefault(i => i.Key == key);
            if (item is not null)
            {
                _items.Remove(item);
                OnCartChanged?.Invoke();
            }
        }

        public void UpdateQuantity(string key, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.Key == key);
            if (item is not null)
            {
                if (quantity <= 0)
                    _items.Remove(item);
                else
                    item.Quantity = quantity;
                OnCartChanged?.Invoke();
            }
        }

        public void Clear()
        {
            _items.Clear();
            OnCartChanged?.Invoke();
        }
    }
}
