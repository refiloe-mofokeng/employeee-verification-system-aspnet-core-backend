using System.ComponentModel.DataAnnotations;

namespace Employee_Verification_System.Admin.Models
{
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required]
        public int EmployeeID { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Channel { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        // Navigation properties
        //public virtual Employee? Employee { get; set; }
    }
}