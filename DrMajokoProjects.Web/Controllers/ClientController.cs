using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.Web.Services;
using DrMajokoProjects.Web.Models.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrMajokoProjects.Web.Controllers
{
    public class ClientController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IApiService apiService, ILogger<ClientController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Client Dashboard Home
        public async Task<IActionResult> Index(string clientId = "client123") // TODO: Get from session/auth
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>($"api/projects/client/{clientId}");
                var maintenanceRequests = await _apiService.GetAsync<List<MaintenanceRequestViewModel>>("api/maintenance");
                var clientRequests = maintenanceRequests.Where(m => m.ClientId == clientId).ToList();

                var dashboardViewModel = new ClientDashboardViewModel
                {
                    ClientId = clientId,
                    Projects = projects,
                    RecentMaintenanceRequests = clientRequests.OrderByDescending(m => m.CreatedAt).Take(5).ToList(),
                    TotalProjects = projects.Count,
                    ActiveProjects = projects.Count(p => p.Status == "In Progress"),
                    PendingRequests = clientRequests.Count(m => m.Status == "Pending"),
                    CompletedRequests = clientRequests.Count(m => m.Status == "Completed")
                };

                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client dashboard");
                TempData["Error"] = "Failed to load dashboard";
                return View(new ClientDashboardViewModel());
            }
        }

        // My Projects
        public async Task<IActionResult> MyProjects(string clientId = "client123")
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>($"api/projects/client/{clientId}");
                ViewBag.ClientId = clientId;
                return View(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client projects");
                TempData["Error"] = "Failed to load projects";
                return View(new List<ProjectViewModel>());
            }
        }

        // View Project Details
        public async Task<IActionResult> ProjectDetails(string id, string clientId = "client123")
        {
            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{id}");
                var maintenanceRequests = await _apiService.GetAsync<List<MaintenanceRequestViewModel>>($"api/maintenance/project/{id}");
                var documents = await _apiService.GetAsync<List<DocumentViewModel>>($"api/documents/project/{id}");

                var viewModel = new ClientProjectDetailsViewModel
                {
                    Project = project,
                    MaintenanceRequests = maintenanceRequests,
                    Documents = documents
                };

                ViewBag.ClientId = clientId;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading project details {id}");
                TempData["Error"] = "Failed to load project details";
                return RedirectToAction(nameof(MyProjects));
            }
        }

        // Submit Maintenance Request - GET
        public async Task<IActionResult> SubmitMaintenance(string clientId = "client123")
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>($"api/projects/client/{clientId}");
                ViewBag.Projects = projects;
                ViewBag.ClientId = clientId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance request form");
                TempData["Error"] = "Failed to load form";
                return RedirectToAction(nameof(Index));
            }
        }

        // Submit Maintenance Request - POST
        [HttpPost]
        public async Task<IActionResult> SubmitMaintenance(CreateMaintenanceRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>($"api/projects/client/{model.ClientId}");
                ViewBag.Projects = projects;
                return View(model);
            }

            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.ProjectId), "ProjectId");
                content.Add(new StringContent(model.ClientId), "ClientId");
                content.Add(new StringContent(model.Title), "Title");
                content.Add(new StringContent(model.Description), "Description");
                content.Add(new StringContent(model.Priority), "Priority");

                if (model.Photos != null && model.Photos.Any())
                {
                    foreach (var photo in model.Photos)
                    {
                        var fileContent = new StreamContent(photo.OpenReadStream());
                        content.Add(fileContent, "photos", photo.FileName);
                    }
                }

                await _apiService.PostMultipartAsync<MaintenanceRequestViewModel>("api/maintenance", content);
                TempData["Success"] = "Maintenance request submitted successfully! We'll assign a contractor soon.";
                return RedirectToAction(nameof(MyMaintenanceRequests), new { clientId = model.ClientId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting maintenance request");
                TempData["Error"] = "Failed to submit maintenance request";
                return View(model);
            }
        }

        // My Maintenance Requests
        public async Task<IActionResult> MyMaintenanceRequests(string clientId = "client123")
        {
            try
            {
                var allRequests = await _apiService.GetAsync<List<MaintenanceRequestViewModel>>("api/maintenance");
                var clientRequests = allRequests.Where(m => m.ClientId == clientId)
                                               .OrderByDescending(m => m.CreatedAt)
                                               .ToList();

                ViewBag.ClientId = clientId;
                return View(clientRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading maintenance requests");
                TempData["Error"] = "Failed to load maintenance requests";
                return View(new List<MaintenanceRequestViewModel>());
            }
        }

        // View Maintenance Request Details
        public async Task<IActionResult> MaintenanceDetails(string id, string clientId = "client123")
        {
            try
            {
                var request = await _apiService.GetAsync<MaintenanceRequestViewModel>($"api/maintenance/{id}");
                ViewBag.ClientId = clientId;
                return View(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading maintenance request {id}");
                TempData["Error"] = "Failed to load request details";
                return RedirectToAction(nameof(MyMaintenanceRequests));
            }
        }

        // ========== CHAT FUNCTIONALITY ==========

        // My Chats - List all project chats
        public async Task<IActionResult> MyChats(string clientId = "client123")
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>($"api/projects/client/{clientId}");
                var allMessages = await _apiService.GetAsync<List<MessageViewModel>>("api/messages");

                var chatProjects = projects.Select(p => new ChatProjectViewModel
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    UnreadCount = allMessages.Count(m => m.ProjectId == p.Id && !m.IsRead && m.SenderId != clientId),
                    LastMessageTime = allMessages.Where(m => m.ProjectId == p.Id)
                                                .OrderByDescending(m => m.CreatedAt)
                                                .FirstOrDefault()?.CreatedAt ?? DateTime.MinValue
                }).OrderByDescending(c => c.LastMessageTime).ToList();

                ViewBag.ClientId = clientId;
                return View(chatProjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chats");
                TempData["Error"] = "Failed to load chats";
                return View(new List<ChatProjectViewModel>());
            }
        }

        // View Project Chat
        public async Task<IActionResult> ProjectChat(string projectId, string clientId = "client123")
        {
            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projectId}");
                var messages = await _apiService.GetAsync<List<MessageViewModel>>($"api/messages/project/{projectId}");

                ViewBag.Project = project;
                ViewBag.ClientId = clientId;
                return View(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading chat for project {projectId}");
                TempData["Error"] = "Failed to load chat";
                return RedirectToAction(nameof(MyProjects));
            }
        }

        // Send Message - POST
        [HttpPost]
public async Task<IActionResult> SendMessage(SendMessageViewModel model)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.ProjectId), "projectId");      // lowercase
                content.Add(new StringContent(model.SenderId), "senderId");        // lowercase
                content.Add(new StringContent(model.SenderName), "senderName");    // lowercase
                content.Add(new StringContent(model.MessageText ?? ""), "messageText"); // lowercase

                if (model.Attachment != null)
                {
                    var fileContent = new StreamContent(model.Attachment.OpenReadStream());
                    content.Add(fileContent, "attachment", model.Attachment.FileName); // lowercase
                }

                await _apiService.PostMultipartAsync<MessageViewModel>("api/messages", content);
                TempData["Success"] = "Message sent successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                TempData["Error"] = $"Failed to send message: {ex.Message}";
            }
            return RedirectToAction(nameof(ProjectChat), new { projectId = model.ProjectId, clientId = model.SenderId });
        }
        // AJAX endpoint for real-time message updates
        [HttpGet]
        public async Task<IActionResult> GetNewMessages(string projectId, DateTime lastMessageTime)
        {
            try
            {
                var messages = await _apiService.GetAsync<List<MessageViewModel>>($"api/messages/project/{projectId}");
                var newMessages = messages.Where(m => m.CreatedAt > lastMessageTime).ToList();
                return PartialView("_ClientMessagePartial", newMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new messages");
                return Content("");
            }
        }
    }
}