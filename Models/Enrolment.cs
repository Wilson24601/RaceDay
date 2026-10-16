using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }
        [ForeignKey("ParticipantId")]
        public User Participant { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        public int RaceNumber { get; set; }
        public string EnrolmentStatus { get; set; } = "Pending";
    }
}