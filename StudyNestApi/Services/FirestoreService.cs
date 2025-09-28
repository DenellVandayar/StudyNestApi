using Google.Cloud.Firestore;
using StudyNestApi.Services;

namespace StudyNestApi.Services
{
    public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService(IConfiguration configuration)
        {
            // Check if environment variable exists (works on Render or local)
            string credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            if (string.IsNullOrEmpty(credentialsPath))
            {
                throw new Exception("Environment variable GOOGLE_APPLICATION_CREDENTIALS is not set.");
            }

            // Set the environment variable so Firestore can authenticate
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            string projectId = configuration["Firestore:ProjectId"];
            _firestoreDb = FirestoreDb.Create(projectId);
        }

        public FirestoreDb GetFirestoreDb() => _firestoreDb;
    }
}
