using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Models;
using RaceDayMVC.Services;
using System.Text.Json;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Send login request to the API
            var response = await _apiService.PostAsync("auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                // Read the JSON response from the API to get the Role and Token
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                string role = root.GetProperty("role").GetString();
                string token = root.GetProperty("token").GetString();
                int userId = root.GetProperty("userId").GetInt32();

                // Save to Session (This keeps the user logged in)
                HttpContext.Session.SetString("UserEmail", model.Email);
                HttpContext.Session.SetString("UserRole", role);
                HttpContext.Session.SetString("Token", token);
                HttpContext.Session.SetString("UserId", userId.ToString());

                return RedirectToAction("Index", "Home");
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Says: {errorDetails} (Status: {response.StatusCode})";
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Send registration request to the API
            var response = await _apiService.PostAsync("auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                // If registration is successful, redirect to login page
                return RedirectToAction(nameof(Login));
            }

            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Says: {errorDetails} (Status: {response.StatusCode})";
            return View(model);
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login");

            // Fetch current user details from API
            var response = await _apiService.GetAsync($"participants/{userIdString}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var user = JsonSerializer.Deserialize<ProfileViewModel>(jsonString, options);
                return View(user);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Profile
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login");

            string? uploadedImageUrl = model.ProfilePictureUrl; // Keep existing if no new image

            // 1. If a new image was uploaded, send it to the API first
            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var uploadResponse = await _apiService.PostFileAsync("upload", model.ProfileImage);

                if (uploadResponse.IsSuccessStatusCode)
                {
                    var jsonString = await uploadResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var uploadResult = JsonSerializer.Deserialize<ApiUploadModel>(jsonString, options);
                    uploadedImageUrl = uploadResult?.Url;
                }
                else
                {
                    ViewBag.Error = "Failed to upload profile picture.";
                    model.Email = HttpContext.Session.GetString("UserEmail");
                    return View(model);
                }
            }

            // 2. Create the payload for the API
            var apiPayload = new
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                ProfilePictureUrl = uploadedImageUrl
            };

            // 3. Send to the API
            var response = await _apiService.PutAsync($"participants/{userIdString}", apiPayload);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Profile updated successfully!";
                model.ProfilePictureUrl = uploadedImageUrl;
            }
            else
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                ViewBag.Error = $"API Says: {errorDetails}";
            }

            model.Email = HttpContext.Session.GetString("UserEmail");
            return View(model);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}