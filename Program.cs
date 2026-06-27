using Employee_Verification_System.Admin.Data;
using Employee_Verification_System.Admin.Services;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().FullName,
    EnvironmentName = Environments.Development
});

// Manually load the appsettings.json from Firebase folder
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("Firebase/appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with SQLite (for Admins and Verifications only)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    });

// ? CORRECT SERVICE REGISTRATIONS
builder.Services.AddScoped<IFirebaseService, FirebaseService>();


// ? FIXED VerificationService with hybrid approach
builder.Services.AddScoped<IVerificationService, VerificationService>();

// ? ADD Firestore Services
builder.Services.AddSingleton<FirestoreDb>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var credentialsPath = configuration["Firebase:CredentialsPath"];

    if (File.Exists(credentialsPath))
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
    }
    else
    {
        // Fallback for development
        var fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "Firebase/firebase-credentials.json");
        if (File.Exists(fallbackPath))
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fallbackPath);
        }
    }

    var projectId = configuration["Firebase:ProjectId"] ?? "flutter-cc-evs";
    return FirestoreDb.Create(projectId);
});

builder.Services.AddScoped<IFirestoreEmployeeService, FirestoreEmployeeService>();

var app = builder.Build();

// Initialize Firebase Admin SDK
var credentialsPath = app.Configuration["Firebase:CredentialsPath"];
if (File.Exists(credentialsPath))
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile(credentialsPath)
    });
}
else
{
    // Use default credentials (for development)
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.GetApplicationDefault()
    });
}

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Database migrated successfully.");

        await SeedAdminUser(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during initialization.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

async Task SeedAdminUser(ApplicationDbContext context)
{
    if (!context.Admins.Any())
    {
        var admin = new Employee_Verification_System.Admin.Models.Admin
        {
            FirstName = "System",
            LastName = "Administrator",
            Email = "admin@company.com",
            Role = "Super Admin",
            Department = "IT"
        };
        context.Admins.Add(admin);
        await context.SaveChangesAsync();

        // Firebase user creation
        var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
        try
        {
            await auth.GetUserByEmailAsync(admin.Email);
            // User already exists
        }
        catch (FirebaseAdmin.Auth.FirebaseAuthException)
        {
            await auth.CreateUserAsync(new UserRecordArgs
            {
                Email = admin.Email,
                EmailVerified = true,
                Password = "Admin@123",
                DisplayName = $"{admin.FirstName} {admin.LastName}"
            });
        }
    }
}