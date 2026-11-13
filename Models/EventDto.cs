namespace RaceDay.Models
{
    public class EventDto
    {
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public decimal Distance { get; set; }
        public int VenueId { get; set; }
        public int CategoryId { get; set; }

        public string? BannerImageUrl { get; set; }
    }
}