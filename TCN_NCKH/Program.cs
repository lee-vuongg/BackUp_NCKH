using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TCN_NCKH.Models.DBModel;
// Nếu RoleAuthorizationMiddleware là một class tùy chỉnh, đảm bảo namespace của nó ở đây
// using YourProject.Middlewares; // Ví dụ: nếu middleware của bạn nằm trong thư mục Middlewares

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình Services ---

// Cấu hình DbContext
builder.Services.AddDbContext<NghienCuuKhoaHocContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Thêm Session vào dịch vụ
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // thời gian tồn tại session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".YourAppName.Session"; // Đặt tên rõ ràng cho session cookie
});

builder.Services.AddHttpContextAccessor(); // Cần thiết để truy cập HttpContext từ các nơi không phải Controller

// Thêm Controller với Views (MVC)
builder.Services.AddControllersWithViews();

// Cấu hình Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Trang đăng nhập
        options.AccessDeniedPath = "/Home/AccessDenied"; // Trang từ chối truy cập
        options.Cookie.HttpOnly = true; // Cookie chỉ có thể truy cập qua HTTP, không qua client-side script
        options.SlidingExpiration = true; // Gia hạn cookie nếu người dùng hoạt động
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Thời gian hết hạn của cookie (ví dụ: 60 phút)
        options.Cookie.IsEssential = true; // Đánh dấu cookie là cần thiết để tuân thủ GDPR
    });

// Thêm Authorization (phải sau Authentication)
builder.Services.AddAuthorization();

// --- Xây dựng ứng dụng ---
var app = builder.Build();

// --- Cấu hình Middleware Pipeline ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); // Chuyển hướng HTTP sang HTTPS (khuyến nghị cho production)

// 1. Static Files (CSS, JS, Images) - Rất sớm trong pipeline
app.UseStaticFiles(); // Chỉ gọi một lần

// 2. Routing - Xác định endpoint nào sẽ xử lý request
app.UseRouting();

// 3. Session - Session phải được đặt trước Authentication/Authorization để có thể sử dụng
app.UseSession();

// 4. Authentication - Xác định người dùng là ai
app.UseAuthentication();

// 5. Authorization - Xác định người dùng có quyền làm gì
app.UseAuthorization();

// 6. Custom Middleware (nếu có) - Như RoleAuthorizationMiddleware của bạn
// Vị trí này là hợp lý nếu nó cần thông tin xác thực và ủy quyền
// app.UseMiddleware<RoleAuthorizationMiddleware>(); // Bỏ comment nếu bạn có middleware này

// 7. Endpoint Routing - Map các request đến Controller actions
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
