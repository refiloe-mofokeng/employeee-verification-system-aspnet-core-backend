//using Microsoft.EntityFrameworkCore;
//using Employee_Verification_System.Admin.Data;
//using Employee_Verification_System.Admin.Models;

//namespace Employee_Verification_System.Admin.Services
//{
//    public class EmployeeService : IEmployeeService
//    {
//        private readonly ApplicationDbContext _context;

//        public EmployeeService(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<List<Employee>> GetAllEmployeesAsync()
//        {
//            return await _context.Employees.ToListAsync();
//        }

//        public async Task<Employee?> GetEmployeeByIdAsync(int id)
//        {
//            return await _context.Employees
//                .Include(e => e.Verifications)
//                .FirstOrDefaultAsync(e => e.EmployeeID == id);
//        }

//        public async Task<bool> UpdateEmployeeStatusAsync(int employeeId, string status)
//        {
//            var employee = await _context.Employees.FindAsync(employeeId);
//            if (employee == null) return false;

//            employee.EmploymentStatus = status;
//            await _context.SaveChangesAsync();
//            return true;
//        }

//        public async Task<List<Verification>> GetEmployeeVerificationsAsync(int employeeId)
//        {
//            return await _context.Verifications
//                .Where(v => v.EmployeeID == employeeId)
//                .ToListAsync();
//        }
//    }
//}