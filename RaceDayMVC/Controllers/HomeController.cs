using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services; 
using System.Diagnostics;
using System.Text.Json; 

namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;

        // Inject the ApiService
        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            // Fetch races from the API
            var response = await _apiService.GetAsync("races");
            var events = new List<EventViewModel>();

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Uses the ApiEventModel we just created in EventViewModel.cs
                var apiEvents = JsonSerializer.Deserialize<List<ApiEventModel>>(jsonString, options);

                if (apiEvents != null)
                {
                    events = apiEvents.Select(e => new EventViewModel
                    {
                        EventId = e.EventId,
                        EventName = e.EventName,
                        EventDate = e.EventDate,
                        Distance = e.Distance,
                        VenueName = e.Venue?.VenueName ?? "TBA"
                    }).ToList();
                }
            }

            // Pass the list of events to the view
            return View(events);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}