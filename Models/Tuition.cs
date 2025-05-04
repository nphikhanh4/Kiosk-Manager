namespace KioskManagementWebApp.Models
{
    public class Tuition
    {
        public int Id { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}