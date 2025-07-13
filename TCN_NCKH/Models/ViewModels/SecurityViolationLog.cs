namespace TCN_NCKH.Models.ViewModels
{
    public class SecurityViolationLog
    {
        // ID của bản ghi LichSuLamBai mà vi phạm này liên quan đến
        public int LichSuLamBaiId { get; set; }

        // Loại vi phạm (ví dụ: "TabSwitch", "WindowSwitch", "CopyPaste", "FullscreenExit")
        public string ViolationType { get; set; }

        // Tin nhắn mô tả chi tiết về vi phạm
        public string Message { get; set; }
    }
}
