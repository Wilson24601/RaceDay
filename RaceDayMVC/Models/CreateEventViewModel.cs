using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDayMVC.Models
{
    public class CreateEventViewModel
    {
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public decimal Distance { get; set; }

        public int VenueId { get; set; }
        public int CategoryId { get; set; }

        public IFormFile? BannerImage { get; set; }

        // Lists for the dropdowns in the view
        public IEnumerable<SelectListItem>? Venues { get; set; }
        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}