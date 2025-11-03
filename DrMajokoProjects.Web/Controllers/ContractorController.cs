using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.Web.Services;
using DrMajokoProjects.Web.Models.ViewModels;

namespace DrMajokoProjects.Web.Controllers
{
    public class ContractorController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IPerformanceManager _performanceManager;
        private readonly ILogger<ContractorController> _logger;

        public ContractorController(
            IApiService apiService,
            IPerformanceManager performanceManager,
            ILogger<ContractorController> logger)
        {
            _apiService = apiService;
            _performanceManager = performanceManager;
            _logger = logger;
        }

        // ========== DASHBOARD ==========

        public async Task<IActionResult> Dashboard(string contractorId = "contractor123")
        {
            try
            {
                // Get all tasks assigned to contractor
                var allTasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/contractor/{contractorId}");

                // Convert to ContractorTaskViewModel with additional info
                var contractorTasks = new List<ContractorTaskViewModel>();
                foreach (var task in allTasks)
                {
                    var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{task.ProjectId}");
                    var phase = await _apiService.GetAsync<PhaseViewModel>($"api/phases/{task.PhaseId}");

                    // Check if task has quotation
                    var quotations = await _apiService.GetAsync<List<QuotationViewModel>>($"api/quotations/task/{task.Id}");

                    contractorTasks.Add(new ContractorTaskViewModel
                    {
                        Id = task.Id,
                        TaskName = task.TaskName,
                        ProjectId = task.ProjectId,
                        ProjectName = project.Name,
                        PhaseId = task.PhaseId,
                        PhaseName = phase.PhaseName,
                        EstimatedStartDate = task.EstimatedStartDate,
                        EstimatedEndDate = task.EstimatedEndDate,
                        ActualStartDate = task.ActualStartDate,
                        ActualEndDate = task.ActualEndDate,
                        EstimatedCost = task.EstimatedCost,
                        ActualCost = task.ActualCost,
                        Status = task.Status,
                        Priority = task.Priority,
                        CompletionPhotoUrls = task.CompletionPhotoUrls ?? new List<string>(),
                        CompletionNotes = task.CompletionNotes,
                        HasQuotation = quotations.Any()
                    });
                }

                // Get unique projects
                var projectIds = contractorTasks.Select(t => t.ProjectId).Distinct();
                var projects = new List<ProjectViewModel>();
                foreach (var projId in projectIds)
                {
                    var proj = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projId}");
                    projects.Add(proj);
                }

                // Get recent quotations
                var contractorQuotations = await _apiService.GetAsync<List<QuotationViewModel>>($"api/quotations/contractor/{contractorId}");
                var recentQuotations = contractorQuotations.OrderByDescending(q => q.SubmittedAt).Take(5).ToList();

                // Calculate performance
                var overallPerformance = _performanceManager.CalculateOverallPerformance(contractorTasks);
                var phasePerformances = _performanceManager.CalculatePhasePerformance(contractorTasks);

                // Get contractor surveys for average rating
                var surveys = await _apiService.GetAsync<List<ContractorSurveyViewModel>>($"api/surveys/contractor/{contractorId}");
                var averageRating = surveys.Any() ? surveys.Average(s => s.OverallRating) : 0;

                var viewModel = new ContractorDashboardViewModel
                {
                    ContractorId = contractorId,
                    ContractorName = "Contractor", // TODO: Get from user service
                    AssignedProjects = projects,
                    AllTasks = contractorTasks,
                    OverallPerformance = overallPerformance,
                    PhasePerformances = phasePerformances,
                    RecentQuotations = recentQuotations,
                    TotalTasks = contractorTasks.Count,
                    CompletedTasks = contractorTasks.Count(t => t.Status == "Done"),
                    InProgressTasks = contractorTasks.Count(t => t.Status == "In Progress"),
                    DelayedTasks = contractorTasks.Count(t => t.IsOverdue),
                    PendingQuotations = contractorQuotations.Count(q => q.Status == "Submitted"),
                    AverageRating = averageRating
                };

                ViewBag.ContractorId = contractorId;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contractor dashboard");
                TempData["Error"] = "Failed to load dashboard";
                return View(new ContractorDashboardViewModel());
            }
        }
        // ========== TASKS ==========

        public async Task<IActionResult> Tasks(string contractorId = "contractor123")
        {
            try
            {
                var allTasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/contractor/{contractorId}");

                var contractorTasks = new List<ContractorTaskViewModel>();
                foreach (var task in allTasks)
                {
                    var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{task.ProjectId}");
                    var phase = await _apiService.GetAsync<PhaseViewModel>($"api/phases/{task.PhaseId}");
                    var quotations = await _apiService.GetAsync<List<QuotationViewModel>>($"api/quotations/task/{task.Id}");

                    contractorTasks.Add(new ContractorTaskViewModel
                    {
                        Id = task.Id,
                        TaskName = task.TaskName,
                        ProjectId = task.ProjectId,
                        ProjectName = project.Name,
                        PhaseId = task.PhaseId,
                        PhaseName = phase.PhaseName,
                        EstimatedStartDate = task.EstimatedStartDate,
                        EstimatedEndDate = task.EstimatedEndDate,
                        ActualStartDate = task.ActualStartDate,
                        ActualEndDate = task.ActualEndDate,
                        EstimatedCost = task.EstimatedCost,
                        ActualCost = task.ActualCost,
                        Status = task.Status,
                        Priority = task.Priority,
                        CompletionPhotoUrls = task.CompletionPhotoUrls ?? new List<string>(),
                        CompletionNotes = task.CompletionNotes,
                        HasQuotation = quotations.Any()
                    });
                }

                ViewBag.ContractorId = contractorId;
                return View(contractorTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contractor tasks");
                TempData["Error"] = "Failed to load tasks";
                return View(new List<ContractorTaskViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> TaskDetails(string id, string contractorId = "contractor123")
        {
            try
            {
                var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{id}");
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{task.ProjectId}");
                var phase = await _apiService.GetAsync<PhaseViewModel>($"api/phases/{task.PhaseId}");
                var quotations = await _apiService.GetAsync<List<QuotationViewModel>>($"api/quotations/task/{id}");

                var contractorTask = new ContractorTaskViewModel
                {
                    Id = task.Id,
                    TaskName = task.TaskName,
                    ProjectId = task.ProjectId,
                    ProjectName = project.Name,
                    PhaseId = task.PhaseId,
                    PhaseName = phase.PhaseName,
                    EstimatedStartDate = task.EstimatedStartDate,
                    EstimatedEndDate = task.EstimatedEndDate,
                    ActualStartDate = task.ActualStartDate,
                    ActualEndDate = task.ActualEndDate,
                    EstimatedCost = task.EstimatedCost,
                    ActualCost = task.ActualCost,
                    Status = task.Status,
                    Priority = task.Priority,
                    CompletionPhotoUrls = task.CompletionPhotoUrls ?? new List<string>(),
                    CompletionNotes = task.CompletionNotes,
                    HasQuotation = quotations.Any()
                };

                ViewBag.ContractorId = contractorId;
                ViewBag.Quotations = quotations;
                return View(contractorTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading task details {id}");
                TempData["Error"] = "Failed to load task details";
                return RedirectToAction(nameof(Tasks));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(UpdateTaskStatusViewModel model, string contractorId = "contractor123")
        {
            try
            {
                // Upload completion photos if any
                if (model.CompletionPhotos != null && model.CompletionPhotos.Any())
                {
                    var photoContent = new MultipartFormDataContent();
                    foreach (var photo in model.CompletionPhotos)
                    {
                        var fileContent = new StreamContent(photo.OpenReadStream());
                        photoContent.Add(fileContent, "photos", photo.FileName);
                    }
                    await _apiService.PostMultipartAsync<object>($"api/tasks/{model.TaskId}/completion-photos", photoContent);
                }

                // Update task status
                var statusUpdate = new
                {
                    Status = model.Status,
                    ActualCost = model.ActualCost,
                    CompletionNotes = model.CompletionNotes
                };

                await _apiService.PutAsync<object>($"api/tasks/{model.TaskId}/status", statusUpdate);
                TempData["Success"] = "Task status updated successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task status {model.TaskId}");
                TempData["Error"] = "Failed to update task status";
            }

            return RedirectToAction(nameof(TaskDetails), new { id = model.TaskId, contractorId });
        }

        // ========== QUOTATIONS ==========

        public async Task<IActionResult> Quotations(string contractorId = "contractor123")
        {
            try
            {
                var quotations = await _apiService.GetAsync<List<QuotationViewModel>>($"api/quotations/contractor/{contractorId}");

                // Get task names for each quotation
                foreach (var quotation in quotations)
                {
                    try
                    {
                        var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{quotation.TaskId}");
                        var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{task.ProjectId}");
                        quotation.TaskName = task.TaskName;
                    }
                    catch
                    {
                        quotation.TaskName = "Unknown Task";
                    }
                }

                ViewBag.ContractorId = contractorId;
                return View(quotations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contractor quotations");
                TempData["Error"] = "Failed to load quotations";
                return View(new List<QuotationViewModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateQuotation(string contractorId = "contractor123")
        {
            try
            {
                // Get tasks assigned to contractor that don't have approved quotations
                var allTasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/contractor/{contractorId}");
                var availableTasks = new List<TaskViewModel>();

                foreach (var task in allTasks)
                {
                    var quotations = await _apiService.GetAsync<List<QuotationViewModel>>($"api/quotations/task/{task.Id}");
                    var hasApprovedQuote = quotations.Any(q => q.Status == "Approved");

                    if (!hasApprovedQuote)
                    {
                        availableTasks.Add(task);
                    }
                }

                ViewBag.Tasks = availableTasks;
                ViewBag.ContractorId = contractorId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create quotation form");
                TempData["Error"] = "Failed to load form";
                return RedirectToAction(nameof(Quotations));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuotation(CreateContractorQuotationViewModel model, string contractorId = "contractor123")
        {
            if (!ModelState.IsValid)
            {
                var allTasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/contractor/{contractorId}");
                ViewBag.Tasks = allTasks;
                ViewBag.ContractorId = contractorId;
                return View(model);
            }

            try
            {
                // Get task and project info
                var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{model.TaskId}");

                // Upload attachments if any
                var attachmentUrls = new List<string>();
                if (model.Attachments != null && model.Attachments.Any())
                {
                    foreach (var attachment in model.Attachments)
                    {
                        if (attachment.Length > 0)
                        {
                            // TODO: Implement file upload to Firebase Storage
                            // For now, we'll skip file uploads or you can add storage service
                        }
                    }
                }

                // Create quotation
                var quotation = new
                {
                    TaskId = model.TaskId,
                    ProjectId = task.ProjectId,
                    ContractorId = contractorId,
                    ContractorName = model.ContractorName,
                    QuotationType = "Task",
                    MaterialCost = model.MaterialCost,
                    LaborCost = model.LaborCost,
                    TotalAmount = model.MaterialCost + model.LaborCost,
                    Description = model.Description,
                    AttachmentUrls = attachmentUrls,
                    Status = "Submitted",
                    SubmittedAt = DateTime.UtcNow
                };

                await _apiService.PostAsync<object>("api/quotations", quotation);
                TempData["Success"] = "Quotation submitted successfully";
                return RedirectToAction(nameof(Quotations), new { contractorId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quotation");
                TempData["Error"] = "Failed to submit quotation";

                var allTasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/contractor/{contractorId}");
                ViewBag.Tasks = allTasks;
                ViewBag.ContractorId = contractorId;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> QuotationDetails(string id, string contractorId = "contractor123")
        {
            try
            {
                var quotation = await _apiService.GetAsync<QuotationViewModel>($"api/quotations/{id}");
                var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{quotation.TaskId}");
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{task.ProjectId}");

                quotation.TaskName = task.TaskName;

                ViewBag.Task = task;
                ViewBag.Project = project;
                ViewBag.ContractorId = contractorId;
                return View(quotation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading quotation details {id}");
                TempData["Error"] = "Failed to load quotation details";
                return RedirectToAction(nameof(Quotations));
            }
        }

        // ========== PERFORMANCE ==========

        public async Task<IActionResult> Performance(string contractorId = "contractor123")
        {
            try
            {
                var allTasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/contractor/{contractorId}");

                var contractorTasks = new List<ContractorTaskViewModel>();
                foreach (var task in allTasks)
                {
                    var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{task.ProjectId}");
                    var phase = await _apiService.GetAsync<PhaseViewModel>($"api/phases/{task.PhaseId}");

                    contractorTasks.Add(new ContractorTaskViewModel
                    {
                        Id = task.Id,
                        TaskName = task.TaskName,
                        ProjectId = task.ProjectId,
                        ProjectName = project.Name,
                        PhaseId = task.PhaseId,
                        PhaseName = phase.PhaseName,
                        EstimatedStartDate = task.EstimatedStartDate,
                        EstimatedEndDate = task.EstimatedEndDate,
                        ActualStartDate = task.ActualStartDate,
                        ActualEndDate = task.ActualEndDate,
                        Status = task.Status
                    });
                }

                var overallPerformance = _performanceManager.CalculateOverallPerformance(contractorTasks);
                var phasePerformances = _performanceManager.CalculatePhasePerformance(contractorTasks);

                // Get surveys
                var surveys = await _apiService.GetAsync<List<ContractorSurveyViewModel>>($"api/surveys/contractor/{contractorId}");

                ViewBag.OverallPerformance = overallPerformance;
                ViewBag.PhasePerformances = phasePerformances;
                ViewBag.Surveys = surveys;
                ViewBag.ContractorId = contractorId;
                ViewBag.Tasks = contractorTasks;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading performance data");
                TempData["Error"] = "Failed to load performance data";
                return View();
            }
        }
    }
}