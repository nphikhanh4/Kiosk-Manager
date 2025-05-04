namespace KioskManagementWebApp.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}