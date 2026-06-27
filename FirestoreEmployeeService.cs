using Employee_Verification_System.Admin.Models;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Employee_Verification_System.Admin.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseAuth _firebaseAuth;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FirebaseService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();

            // Initialize Firebase if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialsPath = _configuration["Firebase:CredentialsPath"];
                if (File.Exists(credentialsPath))
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath)
                    });
                    Console.WriteLine("FirebaseService: Firebase initialized successfully");
                }
                else
                {
                    Console.WriteLine($"FirebaseService: Credentials file not found at {credentialsPath}");
                    // For development, you can use default app
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.GetApplicationDefault(),
                    });
                }
            }
            _firebaseAuth = FirebaseAuth.DefaultInstance;
        }

        public async Task<string?> LoginAdminAsync(string email, string password)
        {
            try
            {
                // Get Firebase Web API key from configuration
                var apiKey = _configuration["Firebase:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    Console.WriteLine("Firebase API key not found in configuration");
                    return null;
                }

                var loginRequest = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}",
                    loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<FirebaseLoginResponse>(content);

                    if (result != null && !string.IsNullOrEmpty(result.IdToken))
                    {
                        Console.WriteLine($"Firebase login successful for: {email}");
                        return result.IdToken;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Firebase login failed: {response.StatusCode} - {errorContent}");

                    // Provide user-friendly error messages
                    if (errorContent.Contains("EMAIL_NOT_FOUND"))
                    {
                        throw new Exception("Email address not found. Please check your email or register first.");
                    }
                    else if (errorContent.Contains("INVALID_PASSWORD"))
                    {
                        throw new Exception("Invalid password. Please try again.");
                    }
                    else if (errorContent.Contains("USER_DISABLED"))
                    {
                        throw new Exception("This account has been disabled. Please contact administrator.");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoginAdminAsync error: {ex.Message}");
                throw; // Re-throw to handle in controller
            }
        }

        public async Task<bool> RegisterAdminAsync(string email, string password)
        {
            try
            {
                // Method 1: Using Firebase Admin SDK (recommended)
                var userArgs = new UserRecordArgs()
                {
                    Email = email,
                    Password = password,
                    EmailVerified = false,
                    Disabled = false
                };

                var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);
                Console.WriteLine($"Firebase user created: {userRecord.Uid} - {userRecord.Email}");
                return true;
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"FirebaseAuthException in RegisterAdminAsync: {ex.Message}");

                // Provide user-friendly error messages
                if (ex.Message.Contains("email already exists"))
                {
                    throw new Exception("An account with this email already exists.");
                }
                else if (ex.Message.Contains("password"))
                {
                    throw new Exception("Password is too weak. Please use a stronger password.");
                }

                throw new Exception($"Registration failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RegisterAdminAsync error: {ex.Message}");
                throw new Exception($"Registration failed: {ex.Message}");
            }
        }

        public Task LogoutAsync()
        {
            // Firebase Admin SDK doesn't handle client-side logout
            // This is typically handled on the client side (Flutter app or browser)
            // We can invalidate tokens if needed, but typically logout is client-side
            Console.WriteLine("FirebaseService: Logout called (client-side operation)");
            return Task.CompletedTask;
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            try
            {
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                return decodedToken != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token verification failed: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> GetUserEmailAsync(string token)
        {
            try
            {
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                return decodedToken.Claims["email"]?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get user email failed: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetUserIdAsync(string token)
        {
            try
            {
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                return decodedToken.Uid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get user ID failed: {ex.Message}");
                return null;
            }
        }

        // Additional methods for Flutter integration
        public async Task<bool> VerifyEmployeeTokenAsync(string token)
        {
            var isValid = await VerifyTokenAsync(token);
            if (isValid)
            {
                var email = await GetUserEmailAsync(token);
                return !string.IsNullOrEmpty(email);
            }
            return false;
        }

        public async Task<bool> IsAdminUserAsync(string token)
        {
            try
            {
                var email = await GetUserEmailAsync(token);
                if (string.IsNullOrEmpty(email))
                    return false;

                // In a real implementation, you would check against your Admins table
                // For now, we'll assume any authenticated user is an admin
                // You'll need to inject ApplicationDbContext for the real check
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IsAdminUserAsync error: {ex.Message}");
                return false;
            }
        }

        // Get user by email using Firebase Admin SDK
        public async Task<FirebaseUserRecord?> GetUserByEmailAsync(string email)
        {
            try
            {
                var userRecord = await _firebaseAuth.GetUserByEmailAsync(email);
                return new FirebaseUserRecord
                {
                    Uid = userRecord.Uid,
                    Email = userRecord.Email,
                    EmailVerified = userRecord.EmailVerified,
                    DisplayName = userRecord.DisplayName
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserByEmailAsync error: {ex.Message}");
                return null;
            }
        }

        // Delete user (for admin management)
        public async Task<bool> DeleteUserAsync(string uid)
        {
            try
            {
                await _firebaseAuth.DeleteUserAsync(uid);
                Console.WriteLine($"Firebase user deleted: {uid}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteUserAsync error: {ex.Message}");
                return false;
            }
        }
    }

    // Helper classes for JSON deserialization
    public class FirebaseLoginResponse
    {
        [JsonProperty("idToken")]
        public string? IdToken { get; set; }

        [JsonProperty("email")]
        public string? Email { get; set; }

        [JsonProperty("refreshToken")]
        public string? RefreshToken { get; set; }

        [JsonProperty("expiresIn")]
        public string? ExpiresIn { get; set; }

        [JsonProperty("localId")]
        public string? LocalId { get; set; }

        [JsonProperty("registered")]
        public bool? Registered { get; set; }
    }

    public class FirebaseUserRecord
    {
        public string Uid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string? DisplayName { get; set; }
    }
}
