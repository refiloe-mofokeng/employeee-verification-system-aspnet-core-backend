using Employee_Verification_System.Admin.Models;

namespace Employee_Verification_System.Admin.Services
{
    public interface IVerificationService
    {
        Task<List<Verification>> GetAllVerificationsAsync();
        Task<Verification?> GetVerificationByIdAsync(string id);
        Task<bool> ApproveVerificationAsync(string employeeEmail, int adminId);
        Task<bool> RejectVerificationAsync(string employeeEmail, int adminId);
        Task<List<Verification>> GetPendingVerificationsAsync();
        Task<List<Verification>> GetEmployeeVerificationsAsync(string employeeEmail);
        Task<List<Verification>> GetVerificationsByEmployeeEmailAsync(string employeeEmail);
    }
}