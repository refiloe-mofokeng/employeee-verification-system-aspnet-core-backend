using Microsoft.AspNetCore.Mvc;
using Employee_Verification_System.Admin.Services;
using Employee_Verification_System.Admin.Models;

namespace Employee_Verification_System.Admin.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeApiController : ControllerBase
    {
        private readonly IFirestoreEmployeeService _firestoreEmployeeService;
        private readonly IFirebaseService _firebaseService;
        private readonly IVerificationService _verificationService;

        public EmployeeApiController(
            IFirestoreEmployeeService firestoreEmployeeService,
            IFirebaseService firebaseService,
            IVerificationService verificationService)
        {
            _firestoreEmployeeService = firestoreEmployeeService;
            _firebaseService = firebaseService;
            _verificationService = verificationService;
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] TokenRequest request)
        {
            var isValid = await _firebaseService.VerifyEmployeeTokenAsync(request.Token);
            return Ok(new { valid = isValid });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetEmployeeProfile([FromHeader] string authorization)
        {
            var token = authorization?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var email = await _firebaseService.GetUserEmailAsync(token);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            // Get employee from Firestore
            var employee = await _firestoreEmployeeService.GetEmployeeByEmailAsync(email);

            if (employee == null)
                return NotFound();

            return Ok(new
            {
                email = employee.email,
                firstName = employee.firstName,
                lastName = employee.lastName,
                employeeNumber = employee.employeeNumber,
                department = employee.department,
                phoneNumber = employee.phoneNumber,
                isVerified = employee.isVerified,
                location = employee.location,
                site = employee.site
            });
        }

        [HttpGet("verifications")]
        public async Task<IActionResult> GetEmployeeVerifications([FromHeader] string authorization)
        {
            var token = authorization?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var email = await _firebaseService.GetUserEmailAsync(token);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            // Get verification history from SQL Server
            var verifications = await _verificationService.GetEmployeeVerificationsAsync(email);
            return Ok(verifications);
        }

        [HttpPost("request-verification")]
        public async Task<IActionResult> RequestVerification([FromHeader] string authorization)
        {
            var token = authorization?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            var email = await _firebaseService.GetUserEmailAsync(token);
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            // Check if employee exists in Firestore
            var employee = await _firestoreEmployeeService.GetEmployeeByEmailAsync(email);
            if (employee == null)
                return NotFound("Employee not found");

            // Create a verification request in SQL Server
            // Note: You might want to add a method to IVerificationService for this
            return Ok(new { message = "Verification request submitted successfully" });
        }
    }

    public class TokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}