using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Models
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }

        // To hold the existing image URL from the database
        public string? ProfilePictureUrl { get; set; }

        // To handle the new file upload
        public IFormFile? ProfileImage { get; set; }
    }
}