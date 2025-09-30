using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using StudyNestApi.Services;

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

    
    if (!DateTime.TryParse(session.StudyDate, out var parsedDate))
        return BadRequest("Invalid studyDate format. Use yyyy-MM-dd or ISO format.");

    
    var sessionData = new Dictionary<string, object>
    {
        { "Id", string.IsNullOrEmpty(session.Id) ? Guid.NewGuid().ToString() : session.Id },
        { "UserId", session.UserId },
        { "Title", session.Title },
        { "StudyDate", parsedDate } 
    };

    var docRef = _firestore.Collection("studySessions").Document(sessionData["Id"].ToString());
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

    var sessions = snapshot.Documents.Select(d => new StudySession(
        id: d.Id,
        userId: d.GetValue<string>("UserId"),
        title: d.GetValue<string>("Title"),
        studyDate: d.GetValue<DateTime>("StudyDate").ToString("yyyy-MM-dd") // return as string
    )).ToList();

    return Ok(sessions);
}

}
    public class StudySession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } // so only their own dates show
    public string Title { get; set; } // optional, e.g., "Math Revision"
    public DateTime StudyDate { get; set; }
}

}
