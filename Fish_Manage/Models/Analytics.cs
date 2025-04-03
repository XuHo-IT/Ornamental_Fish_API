using System.ComponentModel.DataAnnotations;

namespace Fish_Manage.Models
{
    public class Analytics
    {
        [Key]
        public int AnalyticsId { get; set; }
        public int TotalViews { get; set; }
        public int ActiveSessions { get; set; }
    }
}
