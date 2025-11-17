using Google.Cloud.Firestore;
using System.Threading;
using System.Threading.Tasks;

namespace StudyNestApi.Services
{
    public class StudyReminderService : IHostedService, IDisposable
    {
        private readonly ILogger<StudyReminderService> _logger;
        private readonly IServiceProvider _services;
        private Timer? _timer;

        public StudyReminderService(ILogger<StudyReminderService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Study Reminder Service is starting.");
            // Start the timer, run the check immediately 
            // run it again every 60 seconds.
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            _logger.LogInformation("Study Reminder Service is checking for sessions...");

            
            using (var scope = _services.CreateScope())
            {
                var firestoreService = scope.ServiceProvider.GetRequiredService<FirestoreService>();
                var fcmService = scope.ServiceProvider.GetRequiredService<FcmService>();
                var db = firestoreService.GetFirestoreDb();

                try
                {
                    var now = DateTime.UtcNow;
                    
                    var startTime = Timestamp.FromDateTime(now.AddMinutes(9));
                    var endTime = Timestamp.FromDateTime(now.AddMinutes(12));

                    var query = db.Collection("studySessions")
                        .WhereGreaterThan("StudyDate", startTime)
                        .WhereLessThan("StudyDate", endTime)
                        .WhereEqualTo("NotificationSent", false);

                    var snapshot = await query.GetSnapshotAsync();

                    if (snapshot.Documents.Count == 0)
                    {
                        _logger.LogInformation("No upcoming sessions found.");
                        return;
                    }

                    _logger.LogInformation($"Found {snapshot.Documents.Count} sessions to notify.");
                    foreach (var doc in snapshot.Documents)
                    {
                        var userId = doc.GetValue<string>("UserId");
                        var title = doc.GetValue<string>("Title");
                        if (string.IsNullOrEmpty(userId)) continue;

                        // Get the user's device token
                        var tokenDoc = await db.Collection("userProfiles").Document(userId).GetSnapshotAsync();
                        if (!tokenDoc.Exists)
                        {
                            _logger.LogWarning($"No userProfile found for UserId: {userId}");
                            continue;
                        }

                        var fcmToken = tokenDoc.GetValue<string>("fcmToken");
                        if (string.IsNullOrEmpty(fcmToken))
                        {
                            _logger.LogWarning($"No fcmToken found for UserId: {userId}");
                            continue;
                        }

                        // Send the notification
                        await fcmService.SendNotificationAsync(
                            fcmToken,
                            "Study Reminder",
                            $"Your session '{title}' is starting in 10 minutes!"
                        );

                        // Remove the session to avoid duplicate notifications
                        await doc.Reference.UpdateAsync("NotificationSent", true);
                        _logger.LogInformation($"Sent notification and marked session {doc.Id} as notified.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Study Reminder Service.");
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Study Reminder Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose() => _timer?.Dispose();
    }
}