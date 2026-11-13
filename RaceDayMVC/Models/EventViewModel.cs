namespace RaceDayMVC.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Distance { get; set; }
        public string VenueName { get; set; }
        public string? BannerImageUrl { get; set; }
    }

    public class ApiEventModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Distance { get; set; }
        public ApiVenueModel Venue { get; set; }
        public string? BannerImageUrl { get; set; }
    }

    public class ApiVenueModel
    {
        public int VenueId { get; set; } // <-- ADDED
        public string VenueName { get; set; }
    }

    public class ApiCategoryModel // <-- ADDED NEW CLASS
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}