using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KioskManagementWebApp.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RenterId { get; set; }

        [Required]
        [StringLength(7)]
        public string PaymentMonth { get; set; } // yyyy-MM

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaymentDate { get; set; }

        [ForeignKey("RenterId")]
        public virtual Renter Renter { get; set; }
    }
    public class PayRequest
    {
        public int RenterId { get; set; }
    }
}