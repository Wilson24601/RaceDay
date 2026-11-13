namespace RaceDayMVC.Models
{
    public class ParticipantEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; }
        public string ParticipantEmail { get; set; }
        public string EnrolmentStatus { get; set; }
        public DateTime EnrolmentDate { get; set; }
    }
}