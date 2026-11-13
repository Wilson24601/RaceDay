namespace RaceDayMVC.Models
{
    public class EnrolViewModel
    {
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
    }
}