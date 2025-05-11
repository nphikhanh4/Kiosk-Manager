using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KioskManagementWebApp.Models
{
    public class TuitionFee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(6)]
        public string StudentId { get; set; }

        [Required]
        [Column(TypeName = "DECIMAL(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public bool IsPaid { get; set; } = false;

        public DateTime? PaymentDate { get; set; }

        // Navigation properties
        [ForeignKey("StudentId")]
        public Student Student { get; set; }

        public List<TuitionFeeDetail> TuitionFeeDetails { get; set; }
    }

    public class TuitionFeeDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TuitionFeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; }

        [Required]
        [Column(TypeName = "DECIMAL(18,2)")]
        public decimal SubjectFee { get; set; }

        // Navigation property
        [ForeignKey("TuitionFeeId")]
        public TuitionFee TuitionFee { get; set; }
    }
}