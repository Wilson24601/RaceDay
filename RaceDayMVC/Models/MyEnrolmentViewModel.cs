namespace RaceDayMVC.Models
{
    public class MyEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string VenueName { get; set; }
        public string EnrolmentStatus { get; set; }
    }
}