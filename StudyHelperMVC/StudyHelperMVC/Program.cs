using Microsoft.EntityFrameworkCore;
using StudyHelperMVC.Data;
using StudyHelperMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC
builder.Services.AddControllersWithViews();

// HttpClient + сервисы
builder.Services.AddHttpClient();
builder.Services.AddScoped<PdfTextExtractor>();
builder.Services.AddScoped<Chunker>();
builder.Services.AddScoped<GptService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
