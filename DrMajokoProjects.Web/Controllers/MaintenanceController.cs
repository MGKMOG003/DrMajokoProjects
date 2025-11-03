using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.Web.Services;
using DrMajokoProjects.Web.Models.ViewModels;

namespace DrMajokoProjects.Web.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(IApiService apiService, ILogger<MaintenanceController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var requests = await _apiService.GetAsync<List<MaintenanceRequestViewModel>>("api/maintenance");
                return View(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance requests");
                TempData["Error"] = "Failed to load maintenance requests";
                return View(new List<MaintenanceRequestViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMaintenanceRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.ProjectId), "ProjectId");
                content.Add(new StringContent(model.ClientId), "ClientId");
                content.Add(new StringContent(model.Title), "Title");
                content.Add(new StringContent(model.Description), "Description");
                content.Add(new StringContent(model.Priority), "Priority");

                if (model.Photos != null)
                {
                    foreach (var photo in model.Photos)
                    {
                        var fileContent = new StreamContent(photo.OpenReadStream());
                        content.Add(fileContent, "photos", photo.FileName);
                    }
                }

                await _apiService.PostMultipartAsync<MaintenanceRequestViewModel>("api/maintenance", content);
                TempData["Success"] = "Maintenance request created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance request");
                TempData["Error"] = "Failed to create maintenance request";
                return View(model);
            }
        }
    }
}