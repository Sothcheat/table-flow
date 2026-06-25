namespace TableFlow.Api.Data.Entities
{
    public class TableSession
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public Table Table { get; set; } = null!;
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public SessionStatus SessionStatus { get; set; } = SessionStatus.Open;
        public PaymentMethod? PaymentMethod { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? AmountReceived { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser CreatedBy { get; set; } = null!;
        public List<Order> Orders { get; set; } = new();
    }
}
