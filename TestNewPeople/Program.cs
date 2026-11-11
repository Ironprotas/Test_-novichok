using Microsoft.EntityFrameworkCore;
using TestNewPeople.Data;
using TestNewPeople.Services;

var builder = WebApplication.CreateBuilder(args);

var dataDir = Environment.GetEnvironmentVariable("DATA_DIR");
if (string.IsNullOrWhiteSpace(dataDir))
    dataDir = builder.Environment.ContentRootPath;
Directory.CreateDirectory(dataDir);

var dbPath = Path.Combine(dataDir, "novichok.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddRazorPages();
builder.Services.AddScoped<NewcomerStore>();
builder.Services.AddSingleton<AttentionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeed.EnsureSeededAsync(db);
}

var runningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    if (!runningInContainer)
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
}

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();