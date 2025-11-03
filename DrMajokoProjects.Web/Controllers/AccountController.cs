using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.Web.Services;
using DrMajokoProjects.Web.Models.ViewModels;

namespace DrMajokoProjects.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IApiService apiService, ILogger<AccountController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var loginData = new
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var response = await _apiService.PostAsync<LoginResponseViewModel>("api/auth/login", loginData);

                if (response?.User == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return View(model);
                }

                // Store user info in session
                HttpContext.Session.SetString("UserId", response.User.Id);
                HttpContext.Session.SetString("UserEmail", response.User.Email);
                HttpContext.Session.SetString("UserName", response.User.Name);
                HttpContext.Session.SetString("UserRole", response.User.Role);
                HttpContext.Session.SetString("IdToken", response.IdToken);

                TempData["Success"] = $"Welcome back, {response.User.Name}!";

                // Redirect based on role
                return RedirectBasedOnRole(response.User.Role, returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                ModelState.AddModelError(string.Empty, "Login failed. Please check your credentials.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var registerData = new
                {
                    Email = model.Email,
                    Password = model.Password,
                    Name = model.Name,
                    Role = model.Role,
                    PhoneNumber = model.PhoneNumber
                };

                var response = await _apiService.PostAsync<object>("api/auth/register", registerData);

                TempData["Success"] = "Registration successful! Your account is pending approval.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                ModelState.AddModelError(string.Empty, "Registration failed. Email may already be in use.");
                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "You have been logged out successfully";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectBasedOnRole(string role, string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Project Manager" => RedirectToAction("Index", "PM"),
                "Contractor" => RedirectToAction("Dashboard", "Contractor"),
                "Client" => RedirectToAction("Index", "Client"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}