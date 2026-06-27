using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YourProject.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _db;

        public FirestoreService()
        {
            // ✅ Step 1: Locate the Firebase key file
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Firebase/firebase-credentials.json");

            // ✅ Step 2: Set environment variable for authentication
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

            // ✅ Step 3: Initialize Firestore connection using your Firebase Project ID
            _db = FirestoreDb.Create("flutter-cc-evs");  // e.g., "consultationapp-8a123"
        }

        // ✅ Fetch all users from Firestore
        public async Task<List<Dictionary<string, object>>> GetAllUsersAsync()
        {
            CollectionReference usersRef = _db.Collection("users");
            QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

            List<Dictionary<string, object>> users = new();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    users.Add(doc.ToDictionary());
                }
            }

            return users;
        }
    }
}
