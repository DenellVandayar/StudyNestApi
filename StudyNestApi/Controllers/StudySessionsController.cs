using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using StudyNestApi.Services;
using System.Globalization;

namespace StudyNestApi.Controllers
{
    [ApiController]
    [Route("api/studysessions")]
    public class StudySessionsController : ControllerBase
    {
        private readonly FirestoreDb _firestore;

        public StudySessionsController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetFirestoreDb();
        }

        // ✅ Add a study session
        [HttpPost("add")]
        public async Task<IActionResult> AddSession([FromBody] StudySession session)
        {
            if (session == null)
                return BadRequest("Session cannot be null.");

            if (string.IsNullOrEmpty(session.UserId) || string.IsNullOrEmpty(session.Title) || string.IsNullOrEmpty(session.StudyDate))
                return BadRequest("UserId, Title, and StudyDate must be provided.");

            DateTime sessionDateTime;
            try
            {
                sessionDateTime = DateTime.Parse(session.StudyDate, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
            }
            catch (FormatException)
            {
                return BadRequest("Invalid date format. Please use ISO 8601 format (e.g., yyyy-MM-ddTHH:mm:ss).");
            }

            var sessionId = string.IsNullOrEmpty(session.Id) ? Guid.NewGuid().ToString() : session.Id;

            var sessionData = new Dictionary<string, object>
            {
                { "Id", sessionId },
                { "UserId", session.UserId },
                { "Title", session.Title },
                { "StudyDate", Timestamp.FromDateTime(sessionDateTime) }
            };

            var docRef = _firestore.Collection("studySessions").Document(sessionId);
            await docRef.SetAsync(sessionData);

            return Ok(new { message = "Study session added successfully!" });
        }


        // ✅ Get all study sessions for a user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserSessions(string userId)
        {
            var query = _firestore.Collection("studySessions")
                                  .WhereEqualTo("UserId", userId);

            var snapshot = await query.GetSnapshotAsync();

            var sessions = snapshot.Documents.Select(d =>
            {
              
                Timestamp timestamp = d.GetValue<Timestamp>("StudyDate");
                DateTime dateTime = timestamp.ToDateTime();
                
                return new StudySession
                {
                    Id = d.Id,
                    UserId = d.GetValue<string>("UserId"),
                    Title = d.GetValue<string>("Title"),
                    StudyDate = dateTime.ToString("o", CultureInfo.InvariantCulture)
                };
            }).ToList();

            return Ok(sessions);
        }
    }


    public class StudySession
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } // so only their own dates show
        public string Title { get; set; } // optional, e.g., "Math Revision"
        public string StudyDate { get; set; }
    }

}
