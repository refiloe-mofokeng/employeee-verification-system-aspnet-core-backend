//// Models/Employee.cs
//namespace Employee_Verification_System.Admin.Models
//{
//    public class Employee
//    {
//        public string Id { get; set; } = string.Empty;
//        public string EmployeeID { get; set; } = string.Empty;
//        public string FirstName { get; set; } = string.Empty;
//        public string LastName { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public string Department { get; set; } = string.Empty;
//        public string PhoneNumber { get; set; } = string.Empty;
//        public string EmploymentStatus { get; set; } = "Pending Verification";
//        public DateTime DateRegistered { get; set; } = DateTime.Now;
//        public bool IsVerified { get; set; } = false;
//        public bool IsBiometricCompleted { get; set; } = false;
//        public string Site { get; set; } = string.Empty;
//        public string Location { get; set; } = string.Empty;

//        // Navigation properties
//        public virtual ICollection<Verification> Verifications { get; set; } = new List<Verification>();
//        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
//    }
//}