using System;
using System.Collections.Generic;
using System.ComponentModel; // Thêm dòng này cho DisplayName
using System.ComponentModel.DataAnnotations; // Thêm dòng này cho DisplayFormat

namespace TCN_NCKH.Models.ViewModels // Đảm bảo namespace chính xác như thế này
{
    public class ScoreAnalysisViewModel
    {
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public double LowestScore { get; set; }
        public int TotalCompletedExams { get; set; }
        public List<LichSuLamBaiDetailsViewModel> ExamResults { get; set; } = new List<LichSuLamBaiDetailsViewModel>();

        // Các thuộc tính mà mi đã thêm vào trước đó và Controller đang gán
        public string StudentName { get; set; }
        public int TotalExams { get; set; }
        public int PassedExams { get; set; }
        public List<double?> RecentScores { get; set; } = new List<double?>();
    }

}