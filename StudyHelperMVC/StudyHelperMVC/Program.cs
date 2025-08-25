using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Data;
using StudyHelperMVC.Models;
using StudyHelperMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// ??????????? ? SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// MVC
builder.Services.AddControllersWithViews();

// ???????
builder.Services.AddScoped<PdfTextExtractor>();
builder.Services.AddScoped<GptService>();
builder.Services.AddScoped<Chunker>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
