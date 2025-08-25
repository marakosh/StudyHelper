using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Data;
using StudyHelperMVC.Models;
using StudyHelperMVC.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Core (без UI)
builder.Services.AddIdentityCore<ApplicationUser>(opt =>
{
    opt.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();

// Твои сервисы
builder.Services.AddScoped<PdfTextExtractor>();
builder.Services.AddScoped<GptService>();
builder.Services.AddScoped<Chunker>();
builder.Services.AddHttpClient();

var app = builder.Build();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
