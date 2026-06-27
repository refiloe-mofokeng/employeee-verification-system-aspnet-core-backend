using System.ComponentModel.DataAnnotations;

namespace Employee_Verification_System.Admin.Models
{
    public class Admin
    {
        [Key]
        public int AdminID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Verification> Verifications { get; set; } = new List<Verification>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}