using Employee_Verification_System.Admin.Models;
using Employee_Verification_System.Admin.Services;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Employee_Verification_System.Admin.Controllers
{
    [Authorize]
    public class FirestoreEmployeeController : Controller
    {
        private readonly IFirestoreEmployeeService _firestoreEmployeeService;
        private readonly IFirebaseService _firebaseService;
        private readonly IVerificationService _verificationService;
        private readonly ILogger<FirestoreEmployeeController> _logger;

        public FirestoreEmployeeController(
            IFirestoreEmployeeService firestoreEmployeeService,
            IFirebaseService firebaseService,
            IVerificationService verificationService,
            ILogger<FirestoreEmployeeController> logger)
        {
            _firestoreEmployeeService = firestoreEmployeeService;
            _firebaseService = firebaseService;
            _verificationService = verificationService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _firestoreEmployeeService.GetAllEmployeesAsync();
                _logger.LogInformation("Loaded {Count} employees for display", employees.Count);
                return View("~/Views/FirestoreEmployee/Index.cshtml", employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employees for index view");
                TempData["Error"] = "Error loading employees. Please try again.";
                return View("~/Views/FirestoreEmployee/Index.cshtml", new List<FirestoreEmployee>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Verify(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid employee email" });
                }

                // Get current admin ID from claims
                var adminIdClaim = User.FindFirst("AdminId")?.Value;
                if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
                {
                    _logger.LogWarning("Admin ID not found in claims for user: {User}", User.Identity?.Name);
                    return Json(new { success = false, message = "Admin authentication failed" });
                }

                // Update verification status in Firestore
                var success = await _firestoreEmployeeService.UpdateVerificationStatusAsync(id, true);

                if (success)
                {
                    // Create verification record in SQL Server
                    await _verificationService.ApproveVerificationAsync(id, adminId);

                    _logger.LogInformation("Employee verified successfully: {Email} by admin {AdminId}", id, adminId);
                    return Json(new { success = true, message = "Employee verified successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Employee not found or verification failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying employee: {Email}", id);
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Invalid employee email" });
                }

                // Delete from Firestore
                var firestoreSuccess = await _firestoreEmployeeService.DeleteEmployeeAsync(id);

                if (firestoreSuccess)
                {
                    // Delete from Firebase Auth
                    try
                    {
                        var userRecord = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(id);
                        await FirebaseAuth.DefaultInstance.DeleteUserAsync(userRecord.Uid);
                        _logger.LogInformation("User deleted from Firebase Auth: {Email}", id);
                    }
                    catch (FirebaseAuthException authEx)
                    {
                        _logger.LogWarning(authEx, "Could not delete user from Firebase Auth: {Email}", id);
                        // Continue even if Auth deletion fails
                    }

                    _logger.LogInformation("Employee deleted successfully: {Email}", id);
                    return Json(new { success = true, message = "Employee deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Employee not found in database" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee: {Email}", id);
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                // Try to get employee by email first, then by document ID
                var employee = await _firestoreEmployeeService.GetEmployeeByEmailAsync(id)
                            ?? await _firestoreEmployeeService.GetEmployeeByDocumentIdAsync(id);

                if (employee == null)
                {
                    _logger.LogWarning("Employee not found for ID: {Id}", id);
                    return NotFound();
                }

                // Get verification history
                var verifications = await _verificationService.GetVerificationsByEmployeeEmailAsync(employee.email);

                ViewBag.VerificationHistory = verifications;
                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee details for ID: {Id}", id);
                TempData["Error"] = "Error loading employee details.";
                return RedirectToAction("Index");
            }
        }
    }
}