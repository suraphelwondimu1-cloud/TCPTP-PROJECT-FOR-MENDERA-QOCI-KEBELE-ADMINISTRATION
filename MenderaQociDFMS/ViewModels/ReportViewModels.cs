using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.ViewModels
{
    public class ResidentReportViewModel
    {
        public int TotalResidents { get; set; }
        public int ActiveResidents { get; set; }
        public int InactiveResidents { get; set; }
        public List<SubKebeleResidentCount> BySubKebele { get; set; } = new();
    }

    public class SubKebeleResidentCount
    {
        public string SubKebeleName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class IdReportViewModel
    {
        public int TotalIds { get; set; }
        public int ActiveIds { get; set; }
        public int ExpiredIds { get; set; }
        public int CancelledIds { get; set; }
    }

    public class ExpiringIdReportViewModel
    {
        public int TotalExpiringSoon { get; set; }
        public List<KebeleIdRecord> ExpiringRecords { get; set; } = new();
    }

    public class CorrespondenceReportViewModel
    {
        public int TotalCorrespondences { get; set; }
        public List<DocumentTypeCount> ByDocumentType { get; set; } = new();
    }

    public class DocumentTypeCount
    {
        public string DocumentType { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
