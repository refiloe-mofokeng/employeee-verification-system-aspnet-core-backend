using Google.Cloud.Firestore;

namespace Employee_Verification_System.Admin.Models
{
    [FirestoreData]
    public class FirestoreEmployee
    {
        [FirestoreProperty] public string firstName { get; set; }
        [FirestoreProperty] public string lastName { get; set; }
        [FirestoreProperty] public string email { get; set; }
        [FirestoreProperty] public string department { get; set; }
        [FirestoreProperty] public string employeeNumber { get; set; }
        [FirestoreProperty] public string phoneNumber { get; set; }
        [FirestoreProperty] public string idOrPassportNo { get; set; }
        [FirestoreProperty] public string location { get; set; }
        [FirestoreProperty] public string site { get; set; }
        [FirestoreProperty] public bool isVerified { get; set; }
    }
}