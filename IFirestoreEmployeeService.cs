using Employee_Verification_System.Admin.Models;

namespace Employee_Verification_System.Admin.Services
{
    public interface IFirebaseService
    {
        Task<bool> VerifyTokenAsync(string token);
        Task<string?> GetUserEmailAsync(string token);
        Task<string?> GetUserIdAsync(string token);
        Task<string?> LoginAdminAsync(string email, string password);
        Task<bool> RegisterAdminAsync(string email, string password);
        Task LogoutAsync();
        Task<bool> VerifyEmployeeTokenAsync(string token);
        Task<bool> IsAdminUserAsync(string token);
    }
}