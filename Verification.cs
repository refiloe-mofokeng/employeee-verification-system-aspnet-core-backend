using System.ComponentModel.DataAnnotations;

namespace Employee_Verification_System.Admin.Models
{
    public class Report
    {
        [Key]
        public int ReportID { get; set; }

        [Required]
        public int AdminID { get; set; }

        public DateTime GeneratedAt { get; set; }

        [StringLength(500)]
        public string? ReportData { get; set; }

        // Navigation properties
        public virtual Admin? Admin { get; set; }
    }
}