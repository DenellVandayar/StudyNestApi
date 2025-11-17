using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using StudyNestApi.Services;

namespace StudyNestApi.Controllers
{
    [ApiController]
    [Route("api/notes")]
    public class NotesController : ControllerBase
    {
        private readonly FirestoreService _firestoreService;

        public NotesController(FirestoreService firestoreService)
        {
            _firestoreService = firestoreService;
        }

        // POST: api/notes/add
        [HttpPost("add")]
        public async Task<IActionResult> AddNote([FromBody] NoteRequest note)
        {
            if (note == null || string.IsNullOrEmpty(note.UserId))
                return BadRequest("Note or UserId cannot be null.");

            string noteId = string.IsNullOrEmpty(note.Id) ? Guid.NewGuid().ToString() : note.Id;
            var noteData = new Dictionary<string, object>
            {
                { "Id", noteId },
                { "UserId", note.UserId },
                { "Title", note.Title },
                { "Description", note.Description },
                { "CreatedAt", DateTime.UtcNow } // Store as UTC
            };

            var docRef = _firestoreService.GetFirestoreDb()
                .Collection("notes")
                .Document(); // Auto-generate ID

            await docRef.SetAsync(noteData);

            return Ok(new { message = "Note added successfully!" });
        }

        // GET: api/notes/{userUid}
        [HttpGet("{userUid}")]
        public async Task<IActionResult> GetNotes(string userUid)
        {
            if (string.IsNullOrEmpty(userUid))
                return BadRequest("UserId cannot be null.");

            var notesCollection = _firestoreService.GetFirestoreDb().Collection("notes");
            var query = notesCollection.WhereEqualTo("UserId", userUid);
            var snapshot = await query.GetSnapshotAsync();

            var notes = snapshot.Documents.Select(doc => new
            {
                Id = doc.Id,
                UserId = doc.GetValue<string>("UserId"),
                Title = doc.GetValue<string>("Title"),
                Description = doc.GetValue<string>("Description"),
                CreatedAt = doc.GetValue<DateTime>("CreatedAt") // Serialized correctly
            }).ToList();

            return Ok(notes);
        }
    

    [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateNote(string id, [FromBody] NoteRequest note)
        {
            if (note == null)
                return BadRequest("Note cannot be null.");

            var docRef = _firestoreService.GetFirestoreDb().Collection("notes").Document(id);

            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
                return NotFound("Note not found.");

            var updates = new Dictionary<string, object>
            {
                { "Title", note.Title },
                { "Description", note.Description }
            };

            await docRef.UpdateAsync(updates);
            return Ok(new { message = "Note updated successfully!" });
        }


// ✅ GET: api/notes/note/{id}
[HttpGet("note/{id}")]
public async Task<IActionResult> GetNoteById(string id)
{
    var docRef = _firestoreService.GetFirestoreDb().Collection("notes").Document(id);
    var snapshot = await docRef.GetSnapshotAsync();

    if (!snapshot.Exists)
        return NotFound("Note not found.");

    var note = new
    {
        Id = snapshot.Id,
        UserId = snapshot.GetValue<string>("UserId"),
        Title = snapshot.GetValue<string>("Title"),
        Description = snapshot.GetValue<string>("Description"),
        CreatedAt = snapshot.GetValue<DateTime>("CreatedAt")
    };

    return Ok(note);
}

        // ✅ DELETE: api/notes/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteNote(string id)
        {
            var docRef = _firestoreService.GetFirestoreDb().Collection("notes").Document(id);

            var snapshot = await docRef.GetSnapshotAsync();
            if (!snapshot.Exists)
                return NotFound("Note not found.");

            await docRef.DeleteAsync();
            return Ok(new { message = "Note deleted successfully!" });
        }
    }

    // DTO for adding a note
    public class NoteRequest
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

