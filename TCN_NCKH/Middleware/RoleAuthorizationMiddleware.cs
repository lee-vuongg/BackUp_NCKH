using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    // Cấu hình ánh xạ giữa route và role
    private static readonly Dictionary<string, string> AreaRoleMapping = new Dictionary<string, string>
    {
        { "/admin", "Admin" },
        { "/giaovien", "Giáo viên" },
        { "/hocsinh", "Sinh viên" },
        // Thêm area khác nếu cần
    };

    private static readonly HashSet<string> PublicPaths = new HashSet<string>
{
    "/",
    "/home",
    "/home/index",
    "/auth/login",
    "/auth/register",
    "/home/accessdenied",

    // Thêm các đường dẫn cho "Lĩnh vực nổi bật" (Home Controller)
    "/home/ai",
    "/home/webdevelopment",
    "/home/mobileapp",
    "/home/cloudcomputing",

    // Thêm các đường dẫn cho trang giới thiệu bài thi (Test Controller)
    "/test/csharp",
    "/test/networking",
    "/test/database",
    "/test/cybersecurity",
    "/test/webdevelopment",
    "/test/ai",

    // --- THÊM CÁC ĐƯỜNG DẪN NÀY ĐỂ KHÔNG BỊ CHẶN KHI THI VÀ XEM KẾT QUẢ ---
    "/test/starttest", // Cho phép truy cập trang bắt đầu bài thi
    "/test/submittest", // Cho phép gửi kết quả bài thi (đây là POST, nhưng tốt nhất vẫn thêm vào)
    "/test/result"     // Cho phép truy cập trang kết quả
};

    public RoleAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "/";
        var user = context.User;

        // ✅ Cho phép truy cập các PublicPaths mà không cần kiểm tra quyền
        if (PublicPaths.Contains(path) || PublicPaths.Any(p => path.StartsWith(p + "/")))
        {
            await _next(context);
            return;
        }

        // ✅ Kiểm tra đăng nhập, không chuyển hướng khi đã ở trang login hoặc register
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            // Chỉ chuyển hướng về trang login khi chưa đăng nhập và không phải đang ở trang login hoặc register
            if (path != "/auth/login" && path != "/auth/register")
            {
                Debug.WriteLine($"[Middleware] Chưa đăng nhập, chuyển hướng về trang Login");
                context.Response.Redirect("/Auth/Login");
                return;
            }
        }

        // ✅ Lấy các vai trò của người dùng
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        // ✅ Tìm area tương ứng với path
        var matchedEntry = AreaRoleMapping.FirstOrDefault(entry => path.StartsWith(entry.Key));

        // ✅ Nếu có ánh xạ vai trò cho area này
        if (!string.IsNullOrEmpty(matchedEntry.Key))
        {
            var requiredRole = matchedEntry.Value;

            Debug.WriteLine($"[Middleware] URL yêu cầu quyền: {requiredRole}, Role người dùng: {string.Join(", ", userRoles)}");

            // ✅ Kiểm tra xem người dùng có quyền phù hợp hay không
            if (!userRoles.Contains(requiredRole))
            {
                Debug.WriteLine($"[Middleware] Truy cập bị từ chối: User không có quyền {requiredRole}");
                context.Response.Redirect("/Home/AccessDenied");
                return;
            }
        }

        // ✅ Nếu người dùng có quyền hoặc không có yêu cầu quyền, tiếp tục xử lý request
        await _next(context);
    }
}
