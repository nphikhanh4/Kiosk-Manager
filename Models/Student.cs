using System.ComponentModel.DataAnnotations;

namespace KioskManagementWebApp.Models
{
    public class Student
    {
        [Key]
        [StringLength(6)]
        public string StudentId { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^K.*", ErrorMessage = "Course must start with 'K'")]
        public string Course { get; set; }

        [Required]
        [StringLength(255)]
        public string StudentName { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        // Navigation property
        public List<TuitionFee> TuitionFees { get; set; }
    }
}