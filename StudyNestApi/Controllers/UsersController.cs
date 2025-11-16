using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using StudyNestApi.Services;

namespace StudyNestApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly FirestoreDb _firestore;

        public UsersController(FirestoreService firestoreService)
        {
            _firestore = firestoreService.GetFirestoreDb();
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token))
            {
                return BadRequest("UserId and Token are required.");
            }

            var docRef = _firestore.Collection("userProfiles").Document(request.UserId);
            var data = new Dictionary<string, object>
            {
                { "fcmToken", request.Token }
            };

            await docRef.SetAsync(data, SetOptions.MergeAll);
            return Ok(new { message = "Token registered successfully." });
        }
    }

    public class RegisterTokenRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}