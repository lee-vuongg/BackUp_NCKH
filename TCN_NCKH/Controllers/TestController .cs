using Microsoft.AspNetCore.Mvc;
using TCN_NCKH.Models; // Đảm bảo include namespace của Model
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http; // Để sử dụng Session
using System; // Cần cho Random
using System.Text.Json; // Cần cho JsonSerializer

namespace TCN_NCKH.Controllers
{
    public class TestController : Controller
    {
        // Danh sách câu hỏi mẫu cho các chủ đề
        private static Dictionary<string, List<Question>> _allQuestions = new Dictionary<string, List<Question>>
        {
            { "csharp", new List<Question>
                {
                    new Question { Id = 1, Text = "Trong C#, từ khóa nào dùng để khai báo một lớp (class)?", Options = new List<string> {"object", "struct", "class", "interface"}, CorrectOptionIndex = 2 },
                    new Question { Id = 2, Text = "Loại dữ liệu nào dùng để lưu trữ số nguyên?", Options = new List<string> {"float", "string", "bool", "int"}, CorrectOptionIndex = 3 },
                    new Question { Id = 3, Text = "Phương thức `Main` trong C# có kiểu trả về là gì?", Options = new List<string> {"int", "void", "string", "bool"}, CorrectOptionIndex = 1 },
                    new Question { Id = 4, Text = "Từ khóa nào dùng để kế thừa trong C#?", Options = new List<string> {"inherits", "extends", ":", "implements"}, CorrectOptionIndex = 2 },
                    new Question { Id = 5, Text = "Đâu không phải là một loại vòng lặp trong C#?", Options = new List<string> {"for", "while", "do-while", "repeat-until"}, CorrectOptionIndex = 3 },
                    new Question { Id = 6, Text = "Để xuất ra màn hình console, bạn dùng phương thức nào?", Options = new List<string> {"Console.Print()", "Console.Write()", "Console.WriteLine()", "Console.Log()"}, CorrectOptionIndex = 2 },
                    new Question { Id = 7, Text = "Kiểu dữ liệu `string` trong C# là gì?", Options = new List<string> {"Kiểu giá trị", "Kiểu tham chiếu", "Kiểu số", "Kiểu Boolean"}, CorrectOptionIndex = 1 },
                    new Question { Id = 8, Text = "Trong OOP, tính chất nào cho phép một đối tượng có nhiều hình thái khác nhau?", Options = new List<string> {"Kế thừa", "Đóng gói", "Đa hình", "Trừu tượng"}, CorrectOptionIndex = 2 },
                    new Question { Id = 9, Text = "Để xử lý lỗi, bạn dùng khối lệnh nào?", Options = new List<string> {"try-catch", "if-else", "for-loop", "switch-case"}, CorrectOptionIndex = 0 },
                    new Question { Id = 10, Text = "Từ khóa nào dùng để định nghĩa một hằng số?", Options = new List<string> {"var", "let", "const", "static"}, CorrectOptionIndex = 2 },
                    new Question { Id = 11, Text = "Đâu là cách khai báo mảng đúng trong C#?", Options = new List<string> {"int[] arr = new int[5];", "int arr[];", "array int arr;", "int arr = new int[5];"}, CorrectOptionIndex = 0 },
                    new Question { Id = 12, Text = "Phương thức được gọi tự động khi một đối tượng được tạo là gì?", Options = new List<string> {"Destructor", "Method", "Constructor", "Function"}, CorrectOptionIndex = 2 },
                }
            },
            { "networking", new List<Question>
                {
                    new Question { Id = 1, Text = "Giao thức nào được sử dụng để gửi email?", Options = new List<string> {"HTTP", "FTP", "SMTP", "POP3"}, CorrectOptionIndex = 2 },
                    new Question { Id = 2, Text = "Lớp nào trong mô hình OSI chịu trách nhiệm định tuyến gói tin?", Options = new List<string> {"Lớp vật lý", "Lớp liên kết dữ liệu", "Lớp mạng", "Lớp vận chuyển"}, CorrectOptionIndex = 2 },
                    new Question { Id = 3, Text = "Địa chỉ IP phiên bản 4 (IPv4) có bao nhiêu bit?", Options = new List<string> {"16", "32", "64", "128"}, CorrectOptionIndex = 1 },
                    new Question { Id = 4, Text = "Thiết bị mạng nào hoạt động ở lớp vật lý và chỉ khuếch đại tín hiệu?", Options = new List<string> {"Switch", "Router", "Hub", "Bridge"}, CorrectOptionIndex = 2 },
                    new Question { Id = 5, Text = "Port mặc định cho giao thức HTTP là bao nhiêu?", Options = new List<string> {"21", "23", "80", "443"}, CorrectOptionIndex = 2 },
                }
            },
            { "database", new List<Question>
                {
                    new Question { Id = 1, Text = "Lệnh SQL nào dùng để truy vấn dữ liệu từ bảng?", Options = new List<string> {"INSERT", "UPDATE", "DELETE", "SELECT"}, CorrectOptionIndex = 3 },
                    new Question { Id = 2, Text = "Khóa chính (Primary Key) có đặc điểm gì?", Options = new List<string> {"Cho phép giá trị trùng lặp", "Cho phép giá trị NULL", "Phải là duy nhất và không NULL", "Có thể là bất kỳ trường nào"}, CorrectOptionIndex = 2 },
                    new Question { Id = 3, Text = "Trong chuẩn hóa CSDL, dạng chuẩn 1NF yêu cầu gì?", Options = new List<string> {"Không có thuộc tính đa giá trị", "Không có phụ thuộc bắc cầu", "Không có phụ thuộc bộ phận", "Không có khóa ngoại"}, CorrectOptionIndex = 0 },
                    new Question { Id = 4, Text = "Lệnh SQL nào dùng để thêm bản ghi mới vào bảng?", Options = new List<string> {"ALTER TABLE", "CREATE TABLE", "INSERT INTO", "ADD ROW"}, CorrectOptionIndex = 2 },
                    new Question { Id = 5, Text = "Giao dịch (Transaction) trong CSDL tuân theo thuộc tính nào?", Options = new List<string> {"ACID", "BASE", "CRUD", "REST"}, CorrectOptionIndex = 0 },
                }
            },
            { "cybersecurity", new List<Question>
                {
                    new Question { Id = 1, Text = "Tấn công nào cố gắng chiếm quyền truy cập bằng cách thử hàng loạt mật khẩu?", Options = new List<string> {"Phishing", "DDoS", "Brute Force", "SQL Injection"}, CorrectOptionIndex = 2 },
                    new Question { Id = 2, Text = "Mục đích chính của tường lửa (Firewall) là gì?", Options = new List<string> {"Tăng tốc độ mạng", "Ngăn chặn truy cập trái phép", "Sao lưu dữ liệu", "Phân tích lưu lượng web"}, CorrectOptionIndex = 1 },
                    new Question { Id = 3, Text = "Kỹ thuật nào dùng để mã hóa thông tin để chỉ người được ủy quyền mới đọc được?", Options = new List<string> {"Hashing", "Compression", "Encryption", "Decryption"}, CorrectOptionIndex = 2 },
                    new Question { Id = 4, Text = "Phần mềm độc hại tự nhân bản và lây lan qua mạng được gọi là gì?", Options = new List<string> {"Virus", "Trojan", "Worm", "Spyware"}, CorrectOptionIndex = 2 },
                    new Question { Id = 5, Text = "Giao thức nào thường được sử dụng để duyệt web an toàn?", Options = new List<string> {"HTTP", "FTP", "SMTP", "HTTPS"}, CorrectOptionIndex = 3 },
                }
            },
            { "webdevelopment", new List<Question>
                {
                    new Question { Id = 1, Text = "Thẻ HTML nào dùng để tạo một đoạn văn bản?", Options = new List<string> {"<p>", "<div>", "<span>", "<a>"}, CorrectOptionIndex = 0 },
                    new Question { Id = 2, Text = "Thuộc tính CSS nào dùng để thay đổi màu chữ?", Options = new List<string> {"background-color", "font-color", "color", "text-color"}, CorrectOptionIndex = 2 },
                    new Question { Id = 3, Text = "Ngôn ngữ nào chạy phía trình duyệt để tạo tương tác cho trang web?", Options = new List<string> {"Python", "PHP", "JavaScript", "C#"}, CorrectOptionIndex = 2 },
                    new Question { Id = 4, Text = "Phương thức HTTP nào dùng để lấy dữ liệu từ server?", Options = new List<string> {"POST", "PUT", "DELETE", "GET"}, CorrectOptionIndex = 3 },
                    new Question { Id = 5, Text = "Framework CSS nào phổ biến nhất để tạo website responsive?", Options = new List<string> {"Materialize", "Semantic UI", "Tailwind CSS", "Bootstrap"}, CorrectOptionIndex = 3 },
                }
            },
            { "ai", new List<Question>
                {
                    new Question { Id = 1, Text = "Lĩnh vực nào của AI cho phép máy tính học hỏi từ dữ liệu mà không cần lập trình rõ ràng?", Options = new List<string> {"Robot học", "Thị giác máy tính", "Máy học", "Xử lý ngôn ngữ tự nhiên"}, CorrectOptionIndex = 2 },
                    new Question { Id = 2, Text = "Thuật toán máy học nào thường dùng để phân loại dữ liệu thành hai lớp?", Options = new List<string> {"K-Means", "Linear Regression", "Decision Tree", "Support Vector Machine (SVM)"}, CorrectOptionIndex = 3 },
                    new Question { Id = 3, Text = "Mạng nơ-ron sâu (Deep Neural Network) có bao nhiêu lớp?", Options = new List<string> {"1 lớp", "2 lớp", "Nhiều hơn 1 lớp ẩn", "Không có lớp ẩn"}, CorrectOptionIndex = 2 },
                    new Question { Id = 4, Text = "Khu vực nào trong NLP tập trung vào việc giúp máy tính hiểu ý nghĩa của văn bản?", Options = new List<string> {"Tokenization", "Parsing", "Sentiment Analysis", "Natural Language Understanding (NLU)"}, CorrectOptionIndex = 3 },
                    new Question { Id = 5, Text = "Hệ thống AI nào thường được sử dụng trong xe tự lái để nhận diện đối tượng?", Options = new List<string> {"NLP", "Reinforcement Learning", "Computer Vision", "Expert Systems"}, CorrectOptionIndex = 2 },
                }
            },
        };

        // Các Action hiện tại (đã có từ trước)
        public IActionResult CSharp() { ViewData["Title"] = "Thi Trắc Nghiệm Lập Trình C#"; return View(); }
        public IActionResult Networking() { ViewData["Title"] = "Thi Trắc Nghiệm Mạng Máy Tính"; return View(); }
        public IActionResult Database() { ViewData["Title"] = "Thi Trắc Nghiệm Cơ Sở Dữ Liệu"; return View(); }
        public IActionResult CyberSecurity() { ViewData["Title"] = "Thi Trắc Nghiệm An Ninh Mạng"; return View(); }
        public IActionResult WebDevelopment() { ViewData["Title"] = "Thi Trắc Nghiệm Phát Triển Web"; return View(); }
        public IActionResult AI() { ViewData["Title"] = "Thi Trắc Nghiệm Trí Tuệ Nhân Tạo"; return View(); }

        // --- Action để bắt đầu bài thi ---
        // Sử dụng một tham số 'topic' để xác định bài thi nào
        [HttpGet]
        public IActionResult StartTest(string topic)
        {
            if (string.IsNullOrEmpty(topic) || !_allQuestions.ContainsKey(topic.ToLower()))
            {
                // Xử lý trường hợp topic không hợp lệ hoặc không tìm thấy
                return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
            }

            var selectedTopic = topic.ToLower();
            var questionsForTopic = _allQuestions[selectedTopic];

            // Lấy ngẫu nhiên 10 câu hỏi (hoặc ít hơn nếu tổng số câu ít hơn 10)
            var random = new Random();
            var testQuestions = questionsForTopic.OrderBy(q => random.Next()).Take(10).ToList();

            if (testQuestions.Count == 0)
            {
                // Không có câu hỏi nào cho chủ đề này, xử lý lỗi hoặc thông báo
                TempData["ErrorMessage"] = "Không có câu hỏi nào cho chủ đề này.";
                return RedirectToAction("Index", "Home");
            }

            // Lưu các câu hỏi đã chọn vào Session để sử dụng khi chấm điểm
            // Lưu ý: Session sẽ tự động hết hạn, chỉ phù hợp cho lưu trữ tạm thời trong một phiên thi.
            HttpContext.Session.SetString("CurrentTestTopic", selectedTopic);
            HttpContext.Session.SetString("CurrentTestQuestions", System.Text.Json.JsonSerializer.Serialize(testQuestions));

            // Truyền các câu hỏi này đến View
            ViewBag.TopicTitle = GetTopicTitle(selectedTopic);
            return View("Test", testQuestions); // Hiển thị View Test.cshtml
        }

        // --- Action để xử lý việc nộp bài ---
        [HttpPost]
        public IActionResult SubmitTest(IFormCollection form)
        {
            var topic = HttpContext.Session.GetString("CurrentTestTopic");
            var questionsJson = HttpContext.Session.GetString("CurrentTestQuestions");

            if (string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(questionsJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy dữ liệu bài thi. Vui lòng bắt đầu lại.";
                return RedirectToAction("Index", "Home");
            }

            var testQuestions = System.Text.Json.JsonSerializer.Deserialize<List<Question>>(questionsJson);
            if (testQuestions == null || !testQuestions.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy câu hỏi bài thi. Vui lòng bắt đầu lại.";
                return RedirectToAction("Index", "Home");
            }

            int correctAnswers = 0;
            List<QuestionResult> results = new List<QuestionResult>();

            foreach (var q in testQuestions)
            {
                // Lấy đáp án người dùng chọn từ form dựa vào ID câu hỏi
                // Tên input trong form sẽ là "question_{Id}"
                string userAnswerKey = $"question_{q.Id}";
                int? userAnswerIndex = null;

                if (form.ContainsKey(userAnswerKey))
                {
                    if (int.TryParse(form[userAnswerKey], out int parsedAnswerIndex))
                    {
                        userAnswerIndex = parsedAnswerIndex;
                    }
                }

                bool isCorrect = (userAnswerIndex.HasValue && userAnswerIndex.Value == q.CorrectOptionIndex);
                if (isCorrect)
                {
                    correctAnswers++;
                }

                results.Add(new QuestionResult
                {
                    Question = q,
                    UserAnswerIndex = userAnswerIndex,
                    IsCorrect = isCorrect
                });
            }

            // Xóa dữ liệu bài thi khỏi session sau khi nộp
            HttpContext.Session.Remove("CurrentTestTopic");
            HttpContext.Session.Remove("CurrentTestQuestions");

            // Lưu kết quả vào TempData để hiển thị trên trang kết quả
            TempData["Score"] = correctAnswers;
            TempData["TotalQuestions"] = testQuestions.Count;
            TempData["TopicTitle"] = GetTopicTitle(topic);
            TempData["TestResults"] = System.Text.Json.JsonSerializer.Serialize(results);


            return RedirectToAction("Result"); // Chuyển hướng đến trang kết quả
        }

        // Action để hiển thị trang kết quả
        [HttpGet]
        public IActionResult Result()
        {
            if (TempData["Score"] == null)
            {
                // Nếu truy cập trực tiếp trang Result mà không có dữ liệu
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Score = TempData["Score"];
            ViewBag.TotalQuestions = TempData["TotalQuestions"];
            ViewBag.TopicTitle = TempData["TopicTitle"];

            var resultsJson = TempData["TestResults"] as string;
            if (!string.IsNullOrEmpty(resultsJson))
            {
                ViewBag.TestResults = System.Text.Json.JsonSerializer.Deserialize<List<QuestionResult>>(resultsJson);
            }
            else
            {
                ViewBag.TestResults = new List<QuestionResult>();
            }

            return View(); // Hiển thị View Result.cshtml
        }


        // Hàm trợ giúp để lấy tiêu đề chủ đề
        private string GetTopicTitle(string topicKey)
        {
            return topicKey.ToLower() switch
            {
                "csharp" => "Lập Trình C#",
                "networking" => "Mạng Máy Tính",
                "database" => "Cơ Sở Dữ Liệu",
                "cybersecurity" => "An Ninh Mạng",
                "webdevelopment" => "Phát Triển Web",
                "ai" => "Trí Tuệ Nhân Tạo",
                _ => "Bài Thi"
            };
        }
    }

    // Mô hình cho kết quả từng câu hỏi để hiển thị trên trang kết quả
    public class QuestionResult
    {
        public Question Question { get; set; }
        public int? UserAnswerIndex { get; set; }
        public bool IsCorrect { get; set; }
    }
}