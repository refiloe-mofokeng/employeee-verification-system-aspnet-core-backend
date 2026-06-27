using Google.Cloud.Firestore;
using Employee_Verification_System.Admin.Models;

namespace Employee_Verification_System.Admin.Services
{
    public class FirestoreEmployeeService : IFirestoreEmployeeService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<FirestoreEmployeeService> _logger;

        public FirestoreEmployeeService(FirestoreDb firestoreDb, ILogger<FirestoreEmployeeService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task<List<FirestoreEmployee>> GetAllEmployeesAsync()
        {
            try
            {
                var employees = new List<FirestoreEmployee>();
                var snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    if (doc.Exists)
                    {
                        var employee = doc.ConvertTo<FirestoreEmployee>();
                        employees.Add(employee);
                    }
                }

                _logger.LogInformation("Retrieved {Count} employees from Firestore", employees.Count);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees from Firestore");
                throw;
            }
        }

        public async Task<FirestoreEmployee?> GetEmployeeByEmailAsync(string email)
        {
            try
            {
                var query = _firestoreDb.Collection("users").WhereEqualTo("email", email);
                var snapshot = await query.GetSnapshotAsync();

                var document = snapshot.Documents.FirstOrDefault();
                return document?.ConvertTo<FirestoreEmployee>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by email: {Email}", email);
                return null;
            }
        }

        public async Task<FirestoreEmployee?> GetEmployeeByDocumentIdAsync(string documentId)
        {
            try
            {
                var docRef = _firestoreDb.Collection("users").Document(documentId);
                var snapshot = await docRef.GetSnapshotAsync();

                return snapshot.Exists ? snapshot.ConvertTo<FirestoreEmployee>() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee by document ID: {DocumentId}", documentId);
                return null;
            }
        }

        public async Task<bool> UpdateVerificationStatusAsync(string employeeEmail, bool isVerified)
        {
            try
            {
                var query = _firestoreDb.Collection("users").WhereEqualTo("email", employeeEmail);
                var snapshot = await query.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.UpdateAsync("isVerified", isVerified);
                    _logger.LogInformation("Updated verification status for {Email} to {Status}", employeeEmail, isVerified);
                    return true;
                }

                _logger.LogWarning("Employee not found for email: {Email}", employeeEmail);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating verification status for {Email}", employeeEmail);
                return false;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(string employeeEmail)
        {
            try
            {
                var query = _firestoreDb.Collection("users").WhereEqualTo("email", employeeEmail);
                var snapshot = await query.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.DeleteAsync();
                    _logger.LogInformation("Deleted employee from Firestore: {Email}", employeeEmail);
                    return true;
                }

                _logger.LogWarning("Employee not found for deletion: {Email}", employeeEmail);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee: {Email}", employeeEmail);
                return false;
            }
        }

        public async Task<int> GetPendingVerificationsCountAsync()
        {
            try
            {
                var query = _firestoreDb.Collection("users").WhereEqualTo("isVerified", false);
                var snapshot = await query.GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending verifications count");
                return 0;
            }
        }

        public async Task<int> GetVerifiedEmployeesCountAsync()
        {
            try
            {
                var query = _firestoreDb.Collection("users").WhereEqualTo("isVerified", true);
                var snapshot = await query.GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verified employees count");
                return 0;
            }
        }

        public async Task<int> GetTotalEmployeesCountAsync()
        {
            try
            {
                var snapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
                return snapshot.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total employees count");
                return 0;
            }
        }
    }
}