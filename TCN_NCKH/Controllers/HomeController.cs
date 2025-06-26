using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TCN_NCKH.Models;

namespace TCN_NCKH.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // --- Các Action cho "Lĩnh vực nổi bật" ---

        // Trang Trí tuệ nhân tạo
        public IActionResult AI()
        {
            ViewData["Title"] = "Trí tuệ nhân tạo";
            return View(); // Sẽ tìm View ~/Views/Home/AI.cshtml
        }

        // Trang Phát triển Web
        public IActionResult WebDevelopment()
        {
            ViewData["Title"] = "Phát triển Web";
            return View(); // Sẽ tìm View ~/Views/Home/WebDevelopment.cshtml
        }

        // Trang Ứng dụng Di động
        public IActionResult MobileApp()
        {
            ViewData["Title"] = "Ứng dụng Di động";
            return View(); // Sẽ tìm View ~/Views/Home/MobileApp.cshtml
        }

        // Trang Điện toán đám mây
        public IActionResult CloudComputing()
        {
            ViewData["Title"] = "Điện toán đám mây";
            return View(); // Sẽ tìm View ~/Views/Home/CloudComputing.cshtml
        }

    }
}
