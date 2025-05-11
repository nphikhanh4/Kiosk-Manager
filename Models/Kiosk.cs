using System.Collections.Generic;
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
        [Column(TypeName = "decimal(10, 2)")]
        public decimal RentalFee { get; set; }

        public bool IsRented { get; set; }

        public bool Active { get; set; } = true;

        // Navigation property cho mối quan hệ 1-nhiều với Renter
        public virtual ICollection<Renter> Renters { get; set; } = new List<Renter>();
    }
}