namespace RaceDay.Models
{
    public class RegisterDto
    {
        // Properties for user registration
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } 
    }
}