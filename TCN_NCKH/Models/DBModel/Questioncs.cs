namespace TCN_NCKH.Models // Thay thế bằng namespace dự án của bạn
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } // Đã được khởi tạo trong constructor
        public List<string> Options { get; set; } // Đã được khởi tạo trong constructor
        public int CorrectOptionIndex { get; set; }

        public Question()
        {
            // Khởi tạo các thuộc tính không null ở đây
            Text = string.Empty; // Khởi tạo Text bằng một chuỗi rỗng
            Options = new List<string>(); // Đảm bảo Options là một List mới
        }
    }
}