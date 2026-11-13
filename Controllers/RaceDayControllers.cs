using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RaceDay.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Controllers
{
    [Route("api")]
    [ApiController]
    public class RaceDayController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public RaceDayController(RaceDayDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }


        // AUTHENTICATION ENDPOINTS


        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict("Email already exists");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Role = request.Role
            };

            // Hash the password using the built-in hasher
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return StatusCode(201, "User registered successfully");
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid credentials");

            // Verify the password 
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials");


            return Ok(new { Token = "dummy-token-for-swagger-testing", Role = user.Role, UserId = user.UserId });
        }


        // PARTICIPANT ENDPOINTS

        [HttpGet("participants")]
        public async Task<IActionResult> GetParticipants()
        {
            return Ok(await _context.Users.Where(u => u.Role == "Participant").ToListAsync());
        }

        [HttpGet("participants/{id}")]
        public async Task<IActionResult> GetParticipant(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("participants/{id}")]
        public async Task<IActionResult> UpdateParticipant(int id, [FromBody] UpdateProfileDto request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.ProfilePictureUrl = request.ProfilePictureUrl;

            await _context.SaveChangesAsync();
            return Ok("Profile updated successfully");
        }


        // VENUE ENDPOINTS

        [HttpGet("venues")]
        public async Task<IActionResult> GetVenues()
        {
            return Ok(await _context.Venues.ToListAsync());
        }

        [HttpPost("venues")]
        public async Task<IActionResult> CreateVenue([FromBody] Venue venue)
        {
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return StatusCode(201, "Venue added");
        }

        // CATEGORY ENDPOINTS

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _context.Categories.ToListAsync());
        }


        // RACE (EVENT) ENDPOINTS


        [HttpGet("races")]
        public async Task<IActionResult> GetRaces()
        {
            return Ok(await _context.Events.Include(e => e.Venue).ToListAsync());
        }

        [HttpGet("races/{id}")]
        public async Task<IActionResult> GetRace(int id)
        {
            var race = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.EventId == id);
            if (race == null) return NotFound();
            return Ok(race);
        }

        [HttpPost("races")]
        public async Task<IActionResult> CreateRace([FromBody] EventDto dto)
        {
            var newEvent = new Event
            {
                EventName = dto.EventName,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                Distance = dto.Distance,
                VenueId = dto.VenueId,
                CategoryId = dto.CategoryId,
                OrganiserId = 1,
                BannerImageUrl = dto.BannerImageUrl
            };
            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return StatusCode(201, "Race added");
        }


        // REGISTRATION (ENROLMENT) ENDPOINTS

        [HttpGet("registrations")]
        public async Task<IActionResult> GetRegistrations()
        {
            return Ok(await _context.Enrolments.ToListAsync());
        }

        [HttpPost("registrations")]
        public async Task<IActionResult> RegisterForRace([FromBody] EnrolmentDto dto)
        {
            var enrolment = new Enrolment
            {
                ParticipantId = dto.ParticipantId,
                EventId = dto.EventId,
                EnrolmentDate = DateTime.Now,
                EnrolmentStatus = "Confirmed"
            };
            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();
            return StatusCode(201, "Registration successful");
        }

        [HttpGet("registrations/race/{raceid}")]
        public async Task<IActionResult> GetResultsByRace(int raceid)
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)
                .Where(r => r.Enrolment.EventId == raceid)
                .ToListAsync();
            return Ok(results);
        }


        // RESULT ENDPOINTS


        [HttpPost("results")]
        public async Task<IActionResult> RecordResult([FromBody] ResultDto dto)
        {
            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                ResultStatus = dto.ResultStatus
            };
            _context.Results.Add(result);
            await _context.SaveChangesAsync();
            return StatusCode(201, "Result recorded");
        }

        // AZURE BLOB STORAGE ENDPOINT

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // NOTE: For the presentation, we will use a mock Azure connection string.
            string connectionString = "DefaultEndpointsProtocol=https;AccountName=mockaccount;AccountKey=mockkey;EndpointSuffix=core.windows.net";
            string containerName = "event-banners";

            try
            {
                // For the demo 
                string mockUrl = $"https://racedaystorage.blob.core.windows.net/{containerName}/{Guid.NewGuid()}.jpg";

                return Ok(new { Url = mockUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}