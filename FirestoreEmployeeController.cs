using Employee_Verification_System.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Verification_System.Admin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IFirestoreEmployeeService _firestoreEmployeeService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IFirestoreEmployeeService firestoreEmployeeService,
            ILogger<DashboardController> logger)
        {
            _firestoreEmployeeService = firestoreEmployeeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var totalEmployees = await _firestoreEmployeeService.GetTotalEmployeesCountAsync();
                var pendingVerifications = await _firestoreEmployeeService.GetPendingVerificationsCountAsync();
                var verifiedEmployees = await _firestoreEmployeeService.GetVerifiedEmployeesCountAsync();

                ViewBag.TotalEmployees = totalEmployees;
                ViewBag.PendingVerifications = pendingVerifications;
                ViewBag.VerifiedEmployees = verifiedEmployees;
                ViewBag.TotalDocuments = totalEmployees; // Since each employee is a document

                _logger.LogInformation("Dashboard stats - Total: {Total}, Pending: {Pending}, Verified: {Verified}",
                    totalEmployees, pendingVerifications, verifiedEmployees);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                ViewBag.Error = $"Error loading dashboard: {ex.Message}";
                return View();
            }
        }

        public async Task<JsonResult> GetDashboardStats()
        {
            try
            {
                var totalEmployees = await _firestoreEmployeeService.GetTotalEmployeesCountAsync();
                var pendingVerifications = await _firestoreEmployeeService.GetPendingVerificationsCountAsync();
                var verifiedEmployees = await _firestoreEmployeeService.GetVerifiedEmployeesCountAsync();

                // For flagged cases, we'll use a simple in-memory counter
                // In production, you'd store this in a database
                var flaggedCases = await GetFlaggedCasesCountAsync();

                return Json(new
                {
                    totalEmployees,
                    pendingVerifications,
                    verifiedEmployees,
                    flaggedCases,
                    lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return Json(new { error = ex.Message });
            }
        }

        // Add this method to track flagged cases
        private static int _flaggedCasesCount = 0; // Simple in-memory storage

        [HttpPost]
        public JsonResult UpdateFlaggedCases([FromBody] FlaggedCasesRequest request)
        {
            if (request.Increment)
            {
                _flaggedCasesCount++;
            }
            else
            {
                _flaggedCasesCount = Math.Max(0, _flaggedCasesCount - 1);
            }

            return Json(new { success = true, flaggedCases = _flaggedCasesCount });
        }

        private Task<int> GetFlaggedCasesCountAsync()
        {
            return Task.FromResult(_flaggedCasesCount);
        }

        public class FlaggedCasesRequest
        {
            public bool Increment { get; set; }
        }
    }
}