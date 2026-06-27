using Employee_Verification_System.Admin.Models;

namespace Employee_Verification_System.Admin.Services
{
    public interface IFirestoreEmployeeService
    {
        Task<List<FirestoreEmployee>> GetAllEmployeesAsync();
        Task<FirestoreEmployee?> GetEmployeeByEmailAsync(string email);
        Task<bool> UpdateVerificationStatusAsync(string employeeEmail, bool isVerified);
        Task<bool> DeleteEmployeeAsync(string employeeEmail);
        Task<int> GetPendingVerificationsCountAsync();
        Task<int> GetVerifiedEmployeesCountAsync();
        Task<int> GetTotalEmployeesCountAsync();
        Task<FirestoreEmployee?> GetEmployeeByDocumentIdAsync(string documentId);
    }
}