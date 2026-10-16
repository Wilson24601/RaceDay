using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }
        [Required]
        public string VenueName { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
    }
}