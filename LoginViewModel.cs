using System.ComponentModel.DataAnnotations;

namespace Employee_Verification_System.Admin.Models
{
    public class FraudDetection
    {
        [Key]
        public string DetectionID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string EmployeeID { get; set; } = string.Empty;

        [Required]
        public string SuspicionReason { get; set; } = string.Empty;

        public DateTime DetectionDate { get; set; }

        [StringLength(200)]
        public string? ActionTaken { get; set; }
    }
}