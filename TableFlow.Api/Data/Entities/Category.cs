namespace TableFlow.Api.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<MenuItem> MenuItems { get; set; } = new();
    }
}
