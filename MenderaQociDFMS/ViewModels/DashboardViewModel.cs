using MenderaQociDFMS.Models;

namespace MenderaQociDFMS.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalResidents { get; set; }
        public int ActiveResidents { get; set; }
        public int ActiveKebeleIds { get; set; }
        public int ExpiredKebeleIds { get; set; }
        public int IdsExpiringSoon { get; set; }
        public int TotalCorrespondences { get; set; }
        public int StaffUsersCount { get; set; }

        public List<Resident> RecentResidents { get; set; } = new();
        public List<KebeleIdRecord> RecentIdRecords { get; set; } = new();
    }
}
