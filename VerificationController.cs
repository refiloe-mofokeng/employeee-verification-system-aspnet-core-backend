using Employee_Verification_System.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Verification_System.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IFirestoreEmployeeService _firestoreEmployeeService;
        private readonly IVerificationService _verificationService;

        public HomeController(
            IFirestoreEmployeeService firestoreEmployeeService,
            IVerificationService verificationService)
        {
            _firestoreEmployeeService = firestoreEmployeeService;
            _verificationService = verificationService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var firebaseEmployees = await _firestoreEmployeeService.GetAllEmployeesAsync();
            var pendingVerifications = await _verificationService.GetPendingVerificationsAsync();
            var verifiedEmployees = await _firestoreEmployeeService.GetVerifiedEmployeesCountAsync();

            ViewBag.TotalEmployees = firebaseEmployees.Count;
            ViewBag.PendingVerifications = pendingVerifications.Count;
            ViewBag.VerifiedEmployees = verifiedEmployees;
            ViewBag.FlaggedCases = await GetFlaggedCasesCountAsync();

            return View("~/Views/Home/Dashboard.cshtml", firebaseEmployees);
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public IActionResult Report()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        // Helper method to get flagged cases count
        private async Task<int> GetFlaggedCasesCountAsync()
        {
            // For now, return 0 - you can implement this later
            // This would typically come from a separate flagged cases service
            return 0;
        }
    }
}