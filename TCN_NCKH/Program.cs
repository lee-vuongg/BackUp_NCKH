using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
using TCN_NCKH.Services; // Đảm bảo namespace này chứa IEmailService và EmailService
using QuestPDF.Infrastructure; // Thêm dòng này để truy cập LicenseType

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình Services ---

// Cấu hình DbContext
builder.Services.AddDbContext<NghienCuuKhoaHocContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Thêm Session vào dịch vụ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".YourAppName.Session";
});

builder.Services.AddHttpContextAccessor();

// Thêm Controller với Views (MVC)
builder.Services.AddControllersWithViews();

// Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.Cookie.IsEssential = true;
    });

// Thêm Authorization (phải sau Authentication)
builder.Services.AddAuthorization();

// Đăng ký PdfReportService (giữ nguyên vì nó không có interface)
builder.Services.AddScoped<PdfReportService>();

// SỬA ĐĂNG KÝ EMAILSERVICE TẠI ĐÂY:
// Đăng ký EmailService với interface của nó
builder.Services.AddTransient<IEmailService, EmailService>();

// --- Xây dựng ứng dụng ---
var app = builder.Build();

// --- CẤU HÌNH QUESTPDF LICENSE TẠI ĐÂY ---
QuestPDF.Settings.License = LicenseType.Community; // <--- Dòng này đã đúng vị trí

// Để tắt thông báo license trong console (chỉ làm khi đã cấu hình license đúng)
// QuestPDF.Settings.EnableDebugging = false;


// --- Cấu hình Middleware Pipeline ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=AdminHome}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// --- Chạy ứng dụng ---
app.Run();
