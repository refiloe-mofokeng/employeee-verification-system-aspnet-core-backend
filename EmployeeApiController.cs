using Employee_Verification_System.Admin.Data;
using Employee_Verification_System.Admin.Models;
using Employee_Verification_System.Admin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Employee_Verification_System.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IFirebaseService firebaseService, ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _firebaseService = firebaseService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Login attempt for email: {Email}", model.Email);

                    // Verify credentials with Firebase
                    var token = await _firebaseService.LoginAdminAsync(model.Email, model.Password);

                    if (!string.IsNullOrEmpty(token))
                    {
                        // Verify the token and get user info
                        var isValid = await _firebaseService.VerifyTokenAsync(token);
                        var userEmail = await _firebaseService.GetUserEmailAsync(token);
                        var userId = await _firebaseService.GetUserIdAsync(token);

                        if (isValid && !string.IsNullOrEmpty(userEmail))
                        {
                            // Check if user exists in Admins table
                            var admin = await _context.Admins
                                .FirstOrDefaultAsync(a => a.Email.ToLower() == userEmail.ToLower());

                            if (admin != null)
                            {
                                // Create claims identity
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, userId ?? ""),
                                    new Claim(ClaimTypes.Name, userEmail),
                                    new Claim(ClaimTypes.Role, "Admin"),
                                    new Claim("AdminId", admin.AdminID.ToString()),
                                    new Claim("FullName", $"{admin.FirstName} {admin.LastName}"),
                                    new Claim("FirebaseUid", userId ?? ""),
                                    new Claim("Department", admin.Department)
                                };

                                var claimsIdentity = new ClaimsIdentity(
                                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                                var authProperties = new AuthenticationProperties
                                {
                                    IsPersistent = model.RememberMe,
                                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                                    IssuedUtc = DateTimeOffset.UtcNow,
                                    AllowRefresh = true
                                };

                                await HttpContext.SignInAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme,
                                    new ClaimsPrincipal(claimsIdentity),
                                    authProperties);

                                _logger.LogInformation("Successful login for admin: {Email} (ID: {AdminId})", userEmail, admin.AdminID);

                                TempData["Success"] = $"Welcome back, {admin.FirstName}!";

                                // Redirect to returnUrl or default dashboard
                                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                {
                                    return Redirect(returnUrl);
                                }
                                return RedirectToAction("Dashboard", "Home");
                            }
                            else
                            {
                                _logger.LogWarning("Unauthorized login attempt: {Email} is not an admin", userEmail);
                                ModelState.AddModelError(string.Empty, "You are not authorized as an administrator. Please contact system administrator.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Token verification failed for email: {Email}", model.Email);
                            ModelState.AddModelError(string.Empty, "Invalid token verification.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                        ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Login error for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, $"Login error: {ex.Message}");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity?.Name;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation("User logged out: {Email}", userEmail);
            TempData["Success"] = "You have been logged out successfully.";

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied for user: {User} at path: {Path}",
                User.Identity?.Name, HttpContext.Request.Path);
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Registration attempt for email: {Email}", model.Email);

                    // Check if admin already exists in database
                    var existingAdmin = await _context.Admins
                        .FirstOrDefaultAsync(a => a.Email.ToLower() == model.Email.ToLower());

                    if (existingAdmin != null)
                    {
                        ModelState.AddModelError(string.Empty, "An admin with this email already exists in the system.");
                        return View(model);
                    }

                    // Register user in Firebase
                    var success = await _firebaseService.RegisterAdminAsync(model.Email, model.Password);

                    if (success)
                    {
                        // Create admin record in database - USE FULLY QUALIFIED NAME
                        var newAdmin = new Models.Admin  // ← FIXED HERE
                        {
                            FirstName = "New",
                            LastName = "Administrator",
                            Email = model.Email,
                            Role = "Admin",
                            Department = "IT"
                        };

                        _context.Admins.Add(newAdmin);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation("New admin registered successfully: {Email}", model.Email);

                        TempData["Success"] = "Admin account created successfully! You can now login.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create account in authentication system.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Registration error for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, $"Registration error: {ex.Message}");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string email)
        {
            // Firebase password reset is handled client-side
            // You would typically send a password reset email via Firebase
            TempData["Info"] = "Password reset functionality requires Firebase client-side implementation.";
            return RedirectToAction("Login");
        }
    }
}
