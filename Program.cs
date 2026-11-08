using microblog_heads.Components;
using microblog_heads.Data;
using microblog_heads.Models;
using microblog_heads.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDataProtection()
    .SetApplicationName("Microblog-Heads");

// Build the connection string from the values loaded out of .env
var connStr = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
              $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
              $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
              $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
              $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<CommunityService>();
builder.Services.AddScoped<HashTagService>();
builder.Services.AddScoped<DisplayService>();

var app = builder.Build();

// Apply any pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
