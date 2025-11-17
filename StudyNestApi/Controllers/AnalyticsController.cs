using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using StudyNestApi.Services;

namespace StudyNestApi.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly FirestoreDb _firestore;

        public AnalyticsController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetFirestoreDb();
        }

        [HttpGet("dashboard/{userId}")]
        public async Task<IActionResult> GetDashboardStats(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest("UserId is required.");

            // Count Total Notes
            var notesQuery = _firestore.Collection("notes").WhereEqualTo("UserId", userId);
            var notesSnap = await notesQuery.GetSnapshotAsync();
            int totalNotes = notesSnap.Count;

            //  Count Total Sessions
            var sessionsQuery = _firestore.Collection("studySessions").WhereEqualTo("UserId", userId);
            var sessionsSnap = await sessionsQuery.GetSnapshotAsync();
            int totalSessions = sessionsSnap.Count;

            // Calculate Last 7 Days Activity
            var now = DateTime.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);
            
            // Group by Day of Week
            var recentSessions = sessionsSnap.Documents
                .Select(d => d.GetValue<Timestamp>("StudyDate").ToDateTime())
                .Where(d => d > sevenDaysAgo)
                .GroupBy(d => d.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return Ok(new 
            {
                TotalNotes = totalNotes,
                TotalSessions = totalSessions,
                WeeklyActivity = recentSessions
            });
        }
    }
}