using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext
builder.Services.AddDbContext<NghienCuuKhoaHocContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Thêm session vào dịch vụ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // thời gian tồn tại session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

// Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Trang đăng nhập
        options.AccessDeniedPath = "/Home/AccessDenied"; // Trang từ chối truy cập
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseSession();

app.UseStaticFiles(); // trong Program.cs

// Middleware pipeline
app.UseStaticFiles();

app.UseRouting();

// 🔥 THÊM middleware session trước authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Nếu có RoleAuthorizationMiddleware tự custom
app.UseMiddleware<RoleAuthorizationMiddleware>();

// Cấu hình routing cho Area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=AdminHome}/{action=Index}/{id?}"
);

// Routing mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Run app
app.Run();
