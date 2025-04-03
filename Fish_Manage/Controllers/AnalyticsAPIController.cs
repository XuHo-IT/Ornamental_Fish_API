using Fish_Manage.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fish_Manage.Controllers
{
    [Route("api/Analytics")]
    [ApiController]
    public class AnalyticsAPIController : ControllerBase
    {

        private readonly FishManageContext _context;

        public AnalyticsAPIController(FishManageContext context)
        {
            _context = context;
        }

        [HttpPost("TrackView")]
        public IActionResult TrackView()
        {
            var analytics = _context.Analytics.FirstOrDefault();

            if (analytics == null)
            {
                analytics = new Analytics { TotalViews = 1, ActiveSessions = 0 };
                _context.Analytics.Add(analytics);
            }
            else
            {
                analytics.TotalViews++;
            }

            _context.SaveChanges();
            return Ok(new { totalviews = analytics.TotalViews });
        }


        [HttpPost("StartSession")]
        public IActionResult StartSession()
        {
            var analytics = _context.Analytics.FirstOrDefault();

            if (analytics == null)
            {
                analytics = new Analytics { TotalViews = 0, ActiveSessions = 1 };
                _context.Analytics.Add(analytics);
            }
            else
            {
                analytics.ActiveSessions++;
            }

            _context.SaveChanges();
            return Ok(new { activeSessions = analytics.ActiveSessions });
        }


        [HttpPost("EndSessions")]
        public IActionResult EndSessions()
        {
            var analytics = _context.Analytics.FirstOrDefault();
            if (analytics != null)
            {
                analytics.ActiveSessions = Math.Max(0, analytics.ActiveSessions - 1);
                _context.SaveChanges();
            }
            return Ok(new { activeSessions = analytics?.ActiveSessions ?? 0 });
        }
        [HttpGet("GetAnalytics")]
        public IActionResult GetAnalytics()
        {
            var analytics = _context.Analytics.FirstOrDefault();
            return Ok(analytics);
        }


    }
}
