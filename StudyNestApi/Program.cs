using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using StudyNestApi.Services;


var builder = WebApplication.CreateBuilder(args);

string credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
if (string.IsNullOrEmpty(credentialsPath))
{
    throw new Exception("Environment variable GOOGLE_APPLICATION_CREDENTIALS is not set.");
}
try
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(credentialsPath)
    });
    Console.WriteLine("Firebase Admin SDK initialized successfully.");
}
catch (Exception ex)
{
    
    if (!ex.Message.Contains("already exists")) throw;
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<FirestoreService>();

builder.Services.AddSingleton<FcmService>();
builder.Services.AddHostedService<StudyReminderService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();


var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");

