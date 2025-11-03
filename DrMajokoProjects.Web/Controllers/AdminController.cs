using DrMajokoProjects.Web.Helpers;
using DrMajokoProjects.Web.Models.ViewModels;
using DrMajokoProjects.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DrMajokoProjects.Web.Controllers
{
    [AuthorizeRole("Admin")]
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IApiService apiService, ILogger<AdminController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Dashboard Home - UPDATED WITH STATISTICS
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>("api/projects");

                ViewBag.TotalUsers = users?.Count ?? 0;
                ViewBag.PendingUsers = users?.Count(u => u.Status == "Pending") ?? 0;
                ViewBag.TotalProjects = projects?.Count ?? 0;
                ViewBag.ActiveProjects = projects?.Count(p => p.Status == "In Progress") ?? 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["Error"] = "Failed to load dashboard";

                // Set default values on error
                ViewBag.TotalUsers = 0;
                ViewBag.PendingUsers = 0;
                ViewBag.TotalProjects = 0;
                ViewBag.ActiveProjects = 0;

                return View();
            }
        }

        // ========== User Management ==========

        [HttpGet]
        public async Task<IActionResult> AuthRole()
        {
            try
            {
                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                return View(users?.OrderByDescending(u => u.CreatedAt).ToList() ?? new List<UserViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["Error"] = "Failed to load users";
                return View(new List<UserViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            try
            {
                await _apiService.PutAsync<object>($"api/auth/user/{userId}/approve", new { });
                TempData["Success"] = "User approved successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving user {userId}");
                TempData["Error"] = "Failed to approve user";
            }
            return RedirectToAction(nameof(AuthRole));
        }

        [HttpPost]
        public async Task<IActionResult> DenyUser(string userId)
        {
            try
            {
                await _apiService.PutAsync<object>($"api/auth/user/{userId}/deny", new { });
                TempData["Success"] = "User denied successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error denying user {userId}");
                TempData["Error"] = "Failed to deny user";
            }
            return RedirectToAction(nameof(AuthRole));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(string userId, string role)
        {
            try
            {
                var user = await _apiService.GetAsync<UserViewModel>($"api/auth/user/{userId}");
                user.Role = role;
                await _apiService.PutAsync<object>($"api/auth/user/{userId}", user);
                TempData["Success"] = "User role updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user role {userId}");
                TempData["Error"] = "Failed to update user role";
            }
            return RedirectToAction(nameof(AuthRole));
        }

        // ========== Audit Logs ==========

        [HttpGet]
        public async Task<IActionResult> AuditLog(string userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var logs = await _apiService.GetAsync<List<AuditLogViewModel>>("api/auditlogs");

                // Apply filters
                if (!string.IsNullOrEmpty(userId))
                    logs = logs.Where(l => l.UserId == userId).ToList();

                if (startDate.HasValue)
                    logs = logs.Where(l => l.Timestamp >= startDate.Value).ToList();

                if (endDate.HasValue)
                    logs = logs.Where(l => l.Timestamp <= endDate.Value).ToList();

                ViewBag.UserId = userId;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;

                return View(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit logs");
                TempData["Error"] = "Failed to load audit logs";
                return View(new List<AuditLogViewModel>());
            }
        }

        // ========== Chat Management ==========

        [HttpGet]
        public async Task<IActionResult> ChatList()
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>("api/projects");
                var messages = await _apiService.GetAsync<List<MessageViewModel>>("api/messages");

                var chatProjects = projects.Select(p => new ChatProjectViewModel
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    UnreadCount = messages.Count(m => m.ProjectId == p.Id && !m.IsRead),
                    LastMessageTime = messages.Where(m => m.ProjectId == p.Id)
                                              .OrderByDescending(m => m.CreatedAt)
                                              .FirstOrDefault()?.CreatedAt ?? DateTime.MinValue
                }).OrderByDescending(c => c.LastMessageTime).ToList();

                return View(chatProjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat list");
                TempData["Error"] = "Failed to load chat list";
                return View(new List<ChatProjectViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> OpenChat(string projectId)
        {
            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projectId}");
                var messages = await _apiService.GetAsync<List<MessageViewModel>>($"api/messages/project/{projectId}");

                ViewBag.Project = project;
                return View(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading chat for project {projectId}");
                TempData["Error"] = "Failed to load chat";
                return RedirectToAction(nameof(ChatList));
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel model)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.ProjectId), "projectId");
                content.Add(new StringContent(model.SenderId), "senderId");
                content.Add(new StringContent(model.SenderName), "senderName");
                content.Add(new StringContent(model.MessageText ?? ""), "messageText");

                if (model.Attachment != null)
                {
                    var fileContent = new StreamContent(model.Attachment.OpenReadStream());
                    content.Add(fileContent, "attachment", model.Attachment.FileName);
                }

                await _apiService.PostMultipartAsync<MessageViewModel>("api/messages", content);
                TempData["Success"] = "Message sent successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                TempData["Error"] = $"Failed to send message: {ex.Message}";
            }
            return RedirectToAction(nameof(OpenChat), new { projectId = model.ProjectId });
        }

        // AJAX endpoint for real-time message updates
        [HttpGet]
        public async Task<IActionResult> GetNewMessages(string projectId, DateTime lastMessageTime)
        {
            try
            {
                var messages = await _apiService.GetAsync<List<MessageViewModel>>($"api/messages/project/{projectId}");
                var newMessages = messages.Where(m => m.CreatedAt > lastMessageTime).ToList();
                return PartialView("_MessagePartial", newMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting new messages");
                return Content("");
            }
        }

        // ========== Project Management ==========

        [HttpGet]
        public async Task<IActionResult> Projects()
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>("api/projects");
                return View(projects?.OrderByDescending(p => p.StartDate).ToList() ?? new List<ProjectViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading projects");
                TempData["Error"] = "Failed to load projects";
                return View(new List<ProjectViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateProject()
        {
            try
            {
                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                ViewBag.Clients = users.Where(u => u.Role == "Client" && u.Status == "Approved").ToList();
                ViewBag.ProjectManagers = users.Where(u => u.Role == "Project Manager" && u.Status == "Approved").ToList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create project form");
                TempData["Error"] = "Failed to load form";
                return RedirectToAction(nameof(Projects));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                ViewBag.Clients = users.Where(u => u.Role == "Client" && u.Status == "Approved").ToList();
                ViewBag.ProjectManagers = users.Where(u => u.Role == "Project Manager" && u.Status == "Approved").ToList();
                return View(model);
            }

            try
            {
                // Create DTO with all fields including Location and ProjectManagerId
                var projectDto = new
                {
                    Name = model.Name,
                    Description = model.Description,
                    ClientId = model.ClientId,
                    ProjectManagerId = model.ProjectManagerId,  // Ensure PM is included
                    Budget = model.Budget,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Location = model.Location  // Include Location
                };

                await _apiService.PostAsync<object>("api/projects", projectDto);
                TempData["Success"] = "Project created successfully";
                return RedirectToAction(nameof(Projects));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                TempData["Error"] = $"Failed to create project: {ex.Message}";

                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                ViewBag.Clients = users.Where(u => u.Role == "Client" && u.Status == "Approved").ToList();
                ViewBag.ProjectManagers = users.Where(u => u.Role == "Project Manager" && u.Status == "Approved").ToList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProject(string id)
        {
            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{id}");
                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                ViewBag.Clients = users.Where(u => u.Role == "Client" && u.Status == "Approved").ToList();
                ViewBag.ProjectManagers = users.Where(u => u.Role == "Project Manager" && u.Status == "Approved").ToList();
                return View(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading project {id}");
                TempData["Error"] = "Failed to load project";
                return RedirectToAction(nameof(Projects));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditProject(string id, ProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                ViewBag.Clients = users.Where(u => u.Role == "Client" && u.Status == "Approved").ToList();
                ViewBag.ProjectManagers = users.Where(u => u.Role == "Project Manager" && u.Status == "Approved").ToList();
                return View(model);
            }

            try
            {
                await _apiService.PutAsync<object>($"api/projects/{id}", model);
                TempData["Success"] = "Project updated successfully";
                return RedirectToAction(nameof(Projects));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project {id}");
                TempData["Error"] = $"Failed to update project: {ex.Message}";

                var users = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                ViewBag.Clients = users.Where(u => u.Role == "Client" && u.Status == "Approved").ToList();
                ViewBag.ProjectManagers = users.Where(u => u.Role == "Project Manager" && u.Status == "Approved").ToList();
                return View(model);
            }
        }

        // Assign Project Manager to existing project
        [HttpPost]
        public async Task<IActionResult> AssignProjectManager(string projectId, string projectManagerId)
        {
            try
            {
                await _apiService.PutAsync<object>($"api/projects/{projectId}/assign-pm", new { ProjectManagerId = projectManagerId });
                TempData["Success"] = "Project Manager assigned successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning PM to project {projectId}");
                TempData["Error"] = "Failed to assign Project Manager";
            }
            return RedirectToAction(nameof(Projects));
        }

        // ========== Reports ==========

        [HttpGet]
        public async Task<IActionResult> ReportPanel(string contractorFilter = null)
        {
            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>("api/projects");
                var contractors = await _apiService.GetAsync<List<ContractorViewModel>>("api/contractors");
                var maintenanceRequests = await _apiService.GetAsync<List<MaintenanceRequestViewModel>>("api/maintenance");

                // Project Budget Summaries
                var projectSummaries = projects.Select(p => new ProjectBudgetSummary
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    TotalBudget = p.Budget,
                    AmountSpent = p.ActualSpent,
                    RemainingBudget = p.Budget - p.ActualSpent,
                    PercentageSpent = p.Budget > 0 ? (p.ActualSpent / p.Budget) * 100 : 0,
                    Status = p.Status
                }).ToList();

                // Contractor Performance
                var contractorPerformances = contractors.Select(c =>
                {
                    var assignedTasks = maintenanceRequests.Where(m => m.AssignedContractorId == c.Id).ToList();
                    var completedTasks = assignedTasks.Where(m => m.Status == "Completed").ToList();

                    return new ContractorPerformance
                    {
                        ContractorId = c.Id,
                        ContractorName = c.Name,
                        TotalTasksAssigned = assignedTasks.Count,
                        TasksCompleted = completedTasks.Count,
                        TasksOnTime = completedTasks.Count, // Simplified - you can add deadline logic
                        TasksDelayed = 0, // Simplified
                        AverageRating = c.Rating,
                        CompletionRate = assignedTasks.Count > 0 ? (double)completedTasks.Count / assignedTasks.Count * 100 : 0
                    };
                }).ToList();

                // Apply contractor filter
                if (!string.IsNullOrEmpty(contractorFilter))
                {
                    contractorPerformances = contractorPerformances
                        .Where(c => c.ContractorName.Contains(contractorFilter, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                var viewModel = new ReportViewModel
                {
                    ProjectSummaries = projectSummaries,
                    ContractorPerformances = contractorPerformances
                };

                ViewBag.ContractorFilter = contractorFilter;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports");
                TempData["Error"] = "Failed to load reports";
                return View(new ReportViewModel
                {
                    ProjectSummaries = new List<ProjectBudgetSummary>(),
                    ContractorPerformances = new List<ContractorPerformance>()
                });
            }
        }
    }
}