using FirebaseAdmin.Messaging;

namespace StudyNestApi.Services
{
    public class FcmService
    {
        private readonly ILogger<FcmService> _logger;

        public FcmService(ILogger<FcmService> logger)
        {
            _logger = logger;
        }

        public async Task SendNotificationAsync(string token, string title, string body)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                }
            };

            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"Successfully sent message: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending FCM message to token: {token}");
            }
        }
    }
}