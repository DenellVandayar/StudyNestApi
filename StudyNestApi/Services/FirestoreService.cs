using Google.Cloud.Firestore;
using StudyNestApi.Services;

namespace StudyNestApi.Services
{
   public class FirestoreService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirestoreService(IConfiguration configuration)
        {
            // Ensure Firestore credentials path is set
            string credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (string.IsNullOrEmpty(credentialsPath))
            {
                throw new Exception("Environment variable GOOGLE_APPLICATION_CREDENTIALS is not set. " +
                                    "Make sure your Firebase service account JSON path is configured.");
            }

            // Set the environment variable so Firestore SDK can authenticate
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            // Try to get ProjectId from appsettings.json
            string projectId = configuration["Firestore:ProjectId"];

            // Fallback: try environment variable GOOGLE_CLOUD_PROJECT (useful for Render)
            if (string.IsNullOrEmpty(projectId))
            {
                projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
            }

            // If still null, throw exception
            if (string.IsNullOrEmpty(projectId))
            {
                throw new Exception("Firestore ProjectId is not set. " +
                                    "Please check appsettings.json or set GOOGLE_CLOUD_PROJECT environment variable.");
            }

            // Create FirestoreDb instance
            _firestoreDb = FirestoreDb.Create(projectId);
        }

        // Getter for FirestoreDb
        public FirestoreDb GetFirestoreDb() => _firestoreDb;
    }
}
