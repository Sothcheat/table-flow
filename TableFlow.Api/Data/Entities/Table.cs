namespace TableFlow.Api.Data.Entities
{
    public class Table
    {
        public int Id { get; set; }
        public int TableNumber { get; set; }
        public TableStatus TableStatus { get; set; } = TableStatus.Available;
        public List<TableSession> TableSessions { get; set; } = new();
    }
}
