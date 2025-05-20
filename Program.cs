using Microsoft.EntityFrameworkCore;
using KioskManagementWebApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ??ng k? DbContext (ch? m?t l?n, v?i c?c t?y ch?n c?n thi?t)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, LogLevel.Information));

// ??ng k? Session
builder.Services.AddDistributedMemoryCache(); // Cung c?p b? nh? cache cho Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian h?t h?n c?a Session
    options.Cookie.HttpOnly = true; // B?o v? cookie
    options.Cookie.IsEssential = true; // Cho ph?p Session ho?t ??ng m? kh?ng c?n ??ng ? cookie
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// S? d?ng Session tr??c UseRouting
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();