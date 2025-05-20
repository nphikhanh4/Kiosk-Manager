    namespace KioskManagementWebApp.Models
{
    public class DocumentManagementViewModel
    {
        public List<Document> Documents { get; set; }
        public int TotalSoldDocuments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalStock { get; set; }
        public Dictionary<int, int> SoldQuantities { get; set; } // Id của Document và số lượng đã bán
    }
    public class MonthlyStatsViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalSoldDocuments { get; set; }
        public decimal TotalRevenue { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("vi-VN"));
    }
}