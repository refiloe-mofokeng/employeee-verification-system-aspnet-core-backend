//using Employee_Verification_System.Admin.Models;
//using Employee_Verification_System.Admin.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace EmployeeVerificationSystem.Admin.Controllers
//{
//    [Authorize]
//    public class EmployeeController(IEmployeeService employeeService) : Controller
//    {
//        private readonly IEmployeeService _employeeService = employeeService;

//        public async Task<IActionResult> Index()
//        {
//            var employees = await _employeeService.GetAllEmployeesAsync();
//            return View(employees);
//        }

//        public async Task<IActionResult> Details(int id)
//        {
//            var employee = await _employeeService.GetEmployeeByIdAsync(id);
//            if (employee == null)
//            {
//                return NotFound();
//            }
//            return View(employee);
//        }

//        [HttpPost]
//        public async Task<IActionResult> UpdateStatus(int employeeId, string status)
//        {
//            var result = await _employeeService.UpdateEmployeeStatusAsync(employeeId, status);
//            if (result)
//            {
//                TempData["Success"] = "Employee status updated successfully.";
//            }
//            else
//            {
//                TempData["Error"] = "Failed to update employee status.";
//            }
//            return RedirectToAction("Details", new { id = employeeId });
//        }
//    }
//}