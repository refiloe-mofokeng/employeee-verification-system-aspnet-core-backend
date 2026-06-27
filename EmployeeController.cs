using Microsoft.AspNetCore.Mvc;
using Employee_Verification_System.Admin.Services;

namespace Employee_Verification_System.Admin.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationApiController : ControllerBase
    {
        private readonly IVerificationService _verificationService;
        private readonly IFirebaseService _firebaseService;
        private readonly IFirestoreEmployeeService _firestoreEmployeeService;

        public VerificationApiController(
            IVerificationService verificationService,
            IFirebaseService firebaseService,
            IFirestoreEmployeeService firestoreEmployeeService)
        {
            _verificationService = verificationService;
            _firebaseService = firebaseService;
            _firestoreEmployeeService = firestoreEmployeeService;
        }

        [HttpPost("initiate")]
        public async Task<IActionResult> InitiateVerification([FromBody] InitiateVerificationRequest request)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!await _firebaseService.VerifyEmployeeTokenAsync(token))
                return Unauthorized();

            // Implementation for initiating verification
            return Ok(new { message = "Verification initiated successfully" });
        }

        [HttpGet("status/{employeeEmail}")] // Changed from employeeId to employeeEmail
        public async Task<IActionResult> GetVerificationStatus(string employeeEmail) // Changed parameter type
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!await _firebaseService.VerifyEmployeeTokenAsync(token))
                return Unauthorized();

            var verifications = await _verificationService.GetEmployeeVerificationsAsync(employeeEmail); // Use email
            var latestVerification = verifications.OrderByDescending(v => v.VerificationDate).FirstOrDefault();

            return Ok(new { status = latestVerification?.Status ?? "Not Started" });
        }

        [HttpGet("employee-status")]
        public async Task<IActionResult> GetCurrentEmployeeVerificationStatus([FromHeader] string authorization)
        {
            var token = authorization?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var email = await _firebaseService.GetUserEmailAsync(token);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            // Get employee from Firestore to check verification status
            var employee = await _firestoreEmployeeService.GetEmployeeByEmailAsync(email);
            if (employee == null)
                return NotFound();

            // Get verification history
            var verifications = await _verificationService.GetEmployeeVerificationsAsync(email);
            var latestVerification = verifications.OrderByDescending(v => v.VerificationDate).FirstOrDefault();

            return Ok(new
            {
                isVerified = employee.isVerified,
                verificationStatus = latestVerification?.Status ?? "Not Started",
                lastVerificationDate = latestVerification?.VerificationDate
            });
        }
    }

    public class InitiateVerificationRequest
    {
        public string VerificationType { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty; // Changed from EmployeeId to EmployeeEmail
    }
}