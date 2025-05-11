using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KioskManagementWebApp.Models
{
    public class Renter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KioskId { get; set; } // Khóa ngoại

        [Required]
        public string RenterName { get; set; }

        [Required]
        public DateTime? RentalStartDate { get; set; }

        public DateTime? RentalEndDate { get; set; }

        public bool IsActive { get; set; }

        // Navigation property (tùy chọn)
        [ForeignKey("KioskId")]
        public virtual Kiosk Kiosk { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class RentRequest
    {
        public int Id { get; set; }
        public string RenterName { get; set; }
        public string RentalStartDate { get; set; }
        public string RentalEndDate { get; set; }
    }
}