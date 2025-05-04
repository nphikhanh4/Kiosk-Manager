using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KioskManagementWebApp.Models
{
    public class Kiosk
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public bool IsRented { get; set; }

        public string? RenterName { get; set; } // Nullable string

        [Required]
        public DateTime RentalStartDate { get; set; }

        public DateTime? RentalEndDate { get; set; } // Nullable DateTime

        [Required]
        public decimal RentalFee { get; set; }
    }
}