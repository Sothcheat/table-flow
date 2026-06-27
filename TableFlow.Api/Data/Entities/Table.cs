namespace TableFlow.Api.Data.Entities
{
    public class Table
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public TableStatus TableStatus { get; set; } = TableStatus.Available;

        // Stable, unguessable identifier baked into the table's static QR code.
        // The QR encodes /menu?t={PublicToken}; it never changes for the life of the table.
        public Guid PublicToken { get; set; } = Guid.NewGuid();

        public List<TableSession> TableSessions { get; set; } = new();
    }
}
