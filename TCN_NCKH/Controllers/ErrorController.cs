 using Microsoft.AspNetCore.Mvc;

    namespace TCN_NCKH.Controllers
    { 

        public class ErrorController : Controller
        {
            [Route("Error/{statusCode}")]
            public IActionResult HttpStatusCodeHandler(int statusCode)
            {
                switch (statusCode)
                {
                    case 404:
                        ViewData["ErrorMessage"] = "Trang không tồn tại hoặc đã bị xóa.";
                        return View("NotFound");

                    case 500:
                        ViewData["ErrorMessage"] = "Lỗi máy chủ. Vui lòng thử lại sau.";
                        return View("DbError");

                    default:
                        ViewData["ErrorMessage"] = $"Đã xảy ra lỗi mã {statusCode}.";
                        return View("Error");
                }
            }

            [Route("Error")]
            public IActionResult Error()
            {
                ViewData["ErrorMessage"] = "Có lỗi không mong muốn xảy ra. Vui lòng thử lại.";
                return View("Error");
            }

            [Route("Error/DbError")]
            public IActionResult DbError()
            {
                ViewData["ErrorMessage"] = TempData["ErrorMessage"] ?? "Lỗi dữ liệu.";
                return View();
            }
        }
    }


