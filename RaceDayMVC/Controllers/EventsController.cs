using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiService _apiService;

        public EventsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Events 
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if user is an Organiser
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var response = await _apiService.GetAsync("races");
            var events = new List<EventViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var apiEvents = JsonSerializer.Deserialize<List<ApiEventModel>>(jsonString, options);

                if (apiEvents != null)
                {
                    events = apiEvents.Select(e => new EventViewModel
                    {
                        EventId = e.EventId,
                        EventName = e.EventName,
                        EventDate = e.EventDate,
                        Distance = e.Distance,
                        VenueName = e.Venue?.VenueName ?? "TBA",
                        BannerImageUrl = e.BannerImageUrl
                    }).ToList();
                }
            }

            return View(events);
        }

        // GET: /Events/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _apiService.GetAsync($"races/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiEvent = JsonSerializer.Deserialize<ApiEventModel>(jsonString, options);

            var eventDetails = new EventViewModel
            {
                EventId = apiEvent.EventId,
                EventName = apiEvent.EventName,
                EventDate = apiEvent.EventDate,
                Distance = apiEvent.Distance,
                VenueName = apiEvent.Venue?.VenueName ?? "TBA",
                BannerImageUrl = apiEvent.BannerImageUrl
            };

            return View(eventDetails);
        }

        // POST: /Events/Enrol
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId)
        {
            // Get the logged-in user's ID from the Session
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var enrolModel = new EnrolViewModel
            {
                ParticipantId = int.Parse(userIdString),
                EventId = eventId,
                EnrolmentDate = DateTime.Now
            };

            // Send to the API
            var response = await _apiService.PostAsync("registrations", enrolModel);

            if (response.IsSuccessStatusCode)
            {
                // If successful, redirect to their enrolments page
                return RedirectToAction(nameof(MyEnrolments));
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Says: {errorDetails} (Status: {response.StatusCode})";

            // If it fails, reload the details page with the error
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        // GET: /Events/MyEnrolments
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var myEnrolments = new List<MyEnrolmentViewModel>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1. Get all registrations from the API
            var regResponse = await _apiService.GetAsync("registrations");
            if (regResponse.IsSuccessStatusCode)
            {
                var regJson = await regResponse.Content.ReadAsStringAsync();
                var allRegistrations = JsonSerializer.Deserialize<List<ApiRegistrationModel>>(regJson, options);

                // 2. Filter for only THIS participant's registrations
                var userRegistrations = allRegistrations?.Where(r => r.ParticipantId == userId).ToList();

                if (userRegistrations != null && userRegistrations.Any())
                {
                    // 3. Get all races so we can find the names and dates
                    var raceResponse = await _apiService.GetAsync("races");
                    if (raceResponse.IsSuccessStatusCode)
                    {
                        var raceJson = await raceResponse.Content.ReadAsStringAsync();
                        var allRaces = JsonSerializer.Deserialize<List<ApiEventModel>>(raceJson, options);

                        // 4. Combine the data into our ViewModel
                        foreach (var reg in userRegistrations)
                        {
                            var race = allRaces?.FirstOrDefault(r => r.EventId == reg.EventId);
                            if (race != null)
                            {
                                myEnrolments.Add(new MyEnrolmentViewModel
                                {
                                    EnrolmentId = reg.EnrolmentId,
                                    EventId = race.EventId,
                                    EventName = race.EventName,
                                    EventDate = race.EventDate,
                                    VenueName = race.Venue?.VenueName ?? "TBA",
                                    EnrolmentStatus = reg.EnrolmentStatus
                                });
                            }
                        }
                    }
                }
            }

            return View(myEnrolments);
        }

        // GET: /Events/EventEnrolments/5
        [HttpGet]
        public async Task<IActionResult> EventEnrolments(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var participants = new List<ParticipantEnrolmentViewModel>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1. Get all registrations for this event
            var regResponse = await _apiService.GetAsync("registrations");
            if (regResponse.IsSuccessStatusCode)
            {
                var regJson = await regResponse.Content.ReadAsStringAsync();
                var allRegistrations = JsonSerializer.Deserialize<List<ApiRegistrationModel>>(regJson, options);

                // Filter for just this event
                var eventRegs = allRegistrations?.Where(r => r.EventId == id).ToList();

                if (eventRegs != null && eventRegs.Any())
                {
                    // 2. Get all users to find their names and emails
                    var userResponse = await _apiService.GetAsync("participants");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var userJson = await userResponse.Content.ReadAsStringAsync();
                        var allUsers = JsonSerializer.Deserialize<List<ApiUserModel>>(userJson, options);

                        // 3. Combine the data
                        foreach (var reg in eventRegs)
                        {
                            var user = allUsers?.FirstOrDefault(u => u.UserId == reg.ParticipantId);
                            if (user != null)
                            {
                                participants.Add(new ParticipantEnrolmentViewModel
                                {
                                    EnrolmentId = reg.EnrolmentId,
                                    ParticipantName = $"{user.FirstName} {user.LastName}",
                                    ParticipantEmail = user.Email,
                                    EnrolmentStatus = reg.EnrolmentStatus,
                                    EnrolmentDate = DateTime.Now
                                });
                            }
                        }
                    }
                }
            }

            // Pass the event ID so we can show a back button later
            ViewBag.EventId = id;
            return View(participants);
        }

        // POST: /Events/SaveResult
        [HttpPost]
        public async Task<IActionResult> SaveResult(RecordResultViewModel model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            // Convert the string finish time (e.g., "02:15:30") into a TimeSpan for the API
            if (!TimeSpan.TryParse(model.FinishTime, out TimeSpan parsedTime))
            {
                TempData["Error"] = "Invalid time format. Please use HH:mm:ss (e.g., 02:15:30).";
                return RedirectToAction(nameof(EventEnrolments), new { id = model.EventId });
            }

            // Create the payload that matches your API's ResultDto
            var apiPayload = new
            {
                EnrolmentId = model.EnrolmentId,
                FinishTime = parsedTime,
                Position = model.Position,
                ResultStatus = "Completed"
            };

            var response = await _apiService.PostAsync("results", apiPayload);

            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Result recorded successfully!";
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"API Says: {errorDetails}";
            }

            // Reload the same event page to show the message
            return RedirectToAction(nameof(EventEnrolments), new { id = model.EventId });
        }

        // GET: /Events/MyResults
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdString);

            var myResults = new List<MyResultViewModel>();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 1. Get user registrations
            var regResponse = await _apiService.GetAsync("registrations");
            if (regResponse.IsSuccessStatusCode)
            {
                var regJson = await regResponse.Content.ReadAsStringAsync();
                var allRegistrations = JsonSerializer.Deserialize<List<ApiRegistrationModel>>(regJson, options);
                var userRegs = allRegistrations?.Where(r => r.ParticipantId == userId).ToList();

                if (userRegs != null && userRegs.Any())
                {
                    // 2. Get all races to map EventId to EventName
                    var raceResponse = await _apiService.GetAsync("races");
                    var allRaces = new List<ApiEventModel>();
                    if (raceResponse.IsSuccessStatusCode)
                    {
                        var raceJson = await raceResponse.Content.ReadAsStringAsync();
                        allRaces = JsonSerializer.Deserialize<List<ApiEventModel>>(raceJson, options);
                    }

                    // 3. Loop through user's registrations to find results
                    foreach (var reg in userRegs)
                    {
                        // Fetch results for this specific race
                        var resultResponse = await _apiService.GetAsync($"registrations/race/{reg.EventId}");
                        if (resultResponse.IsSuccessStatusCode)
                        {
                            var resultJson = await resultResponse.Content.ReadAsStringAsync();
                            var raceResults = JsonSerializer.Deserialize<List<ApiResultModel>>(resultJson, options);

                            // Find the result that belongs to this participant's enrolment
                            var myResult = raceResults?.FirstOrDefault(r => r.EnrolmentId == reg.EnrolmentId);

                            if (myResult != null)
                            {
                                var race = allRaces?.FirstOrDefault(r => r.EventId == reg.EventId);
                                myResults.Add(new MyResultViewModel
                                {
                                    EventName = race?.EventName ?? "Unknown Event",
                                    EventDate = race?.EventDate ?? DateTime.Now,
                                    FinishTime = myResult.FinishTime.ToString(@"hh\:mm\:ss"),
                                    Position = myResult.Position,
                                    ResultStatus = myResult.ResultStatus
                                });
                            }
                        }
                    }
                }
            }

            return View(myResults);
        }

        // GET: /Events/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CreateEventViewModel();
            await PopulateDropdowns(model);

            return View(model);
        }

        private async Task PopulateDropdowns(CreateEventViewModel model)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Fetch Venues
            var venueResponse = await _apiService.GetAsync("venues");
            if (venueResponse.IsSuccessStatusCode)
            {
                var venueJson = await venueResponse.Content.ReadAsStringAsync();
                var venues = JsonSerializer.Deserialize<List<ApiVenueModel>>(venueJson, options);
                model.Venues = venues?.Select(v => new SelectListItem { Value = v.VenueId.ToString(), Text = v.VenueName });
            }

            // Fetch Categories
            var catResponse = await _apiService.GetAsync("categories");
            if (catResponse.IsSuccessStatusCode)
            {
                var catJson = await catResponse.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<ApiCategoryModel>>(catJson, options);
                model.Categories = categories?.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.CategoryName });
            }
        }

        // POST: /Events/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string? uploadedImageUrl = null;

            if (model.BannerImage != null && model.BannerImage.Length > 0)
            {
                var uploadResponse = await _apiService.PostFileAsync("upload", model.BannerImage);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var jsonString = await uploadResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var uploadResult = JsonSerializer.Deserialize<ApiUploadModel>(jsonString, options);
                    uploadedImageUrl = uploadResult?.Url;
                }
                else
                {
                    ViewBag.Error = "Failed to upload image. Please try again.";
                    await PopulateDropdowns(model);
                    return View(model);
                }
            }

            var apiPayload = new
            {
                EventName = model.EventName,
                EventDate = model.EventDate,
                StartTime = model.StartTime,
                Distance = model.Distance,
                VenueId = model.VenueId,
                CategoryId = model.CategoryId,
                BannerImageUrl = uploadedImageUrl
            };

            var response = await _apiService.PostAsync("races", apiPayload);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Says: {errorDetails} (Status: {response.StatusCode})";
            await PopulateDropdowns(model);
            return View(model);
        }
    }

    public class ApiRegistrationModel
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public string EnrolmentStatus { get; set; }
    }

    public class ApiUserModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class ApiResultModel
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public string ResultStatus { get; set; }
    }

    public class ApiUploadModel
    {
        public string Url { get; set; }
    }
}