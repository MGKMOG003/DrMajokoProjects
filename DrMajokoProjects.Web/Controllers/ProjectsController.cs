using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.Web.Services;
using DrMajokoProjects.Web.Models.ViewModels;

namespace DrMajokoProjects.Web.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IApiService apiService, ILogger<ProjectsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>("api/projects");
                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading projects");
                TempData["Error"] = "Failed to load projects";
                return View(new List<ProjectViewModel>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _apiService.PostAsync<ProjectViewModel>("api/projects", model);
                TempData["Success"] = "Project created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                TempData["Error"] = "Failed to create project";
                return View(model);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{id}");
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading project {id}");
                TempData["Error"] = "Failed to load project details";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}