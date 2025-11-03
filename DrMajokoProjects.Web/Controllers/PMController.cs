using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.Web.Services;
using DrMajokoProjects.Web.Models.ViewModels;
using DrMajokoProjects.Web.Helpers;
using System.Text.Json;

namespace DrMajokoProjects.Web.Controllers
{
    [AuthorizeRole("Project Manager", "Admin")]
    public class PMController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PMController> _logger;

        public PMController(IApiService apiService, ILogger<PMController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Helper method to get current user ID from session
        private string GetCurrentUserId()
        {
            return HttpContext.Session.GetString("UserId") ?? "pm123";
        }

        // ========== DASHBOARD ==========

        public async Task<IActionResult> Index()
        {
            var pmId = GetCurrentUserId();

            try
            {
                var projects = await _apiService.GetAsync<List<ProjectViewModel>>($"api/projects/pm/{pmId}");
                var allQuotations = await _apiService.GetAsync<List<QuotationViewModel>>("api/quotations/project/all");
                var allTasks = await _apiService.GetAsync<List<TaskViewModel>>("api/tasks/project/all");

                var dashboardViewModel = new PMDashboardViewModel
                {
                    ProjectManagerId = pmId,
                    Projects = projects,
                    TotalProjects = projects.Count,
                    ActiveProjects = projects.Count(p => p.Status == "In Progress"),
                    PendingQuotations = allQuotations?.Count(q => q.Status == "Submitted") ?? 0,
                    OverdueTasks = allTasks?.Count(t => t.IsOverdue) ?? 0
                };

                ViewBag.PMId = pmId;
                return View(dashboardViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading PM dashboard");
                TempData["Error"] = "Failed to load dashboard";
                return View(new PMDashboardViewModel());
            }
        }

        // ========== TIMELINE / GANTT CHART ==========

        public async Task<IActionResult> Timeline(string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projectId}");
                var phases = await _apiService.GetAsync<List<PhaseViewModel>>($"api/phases/project/{projectId}");

                // Get tasks for each phase
                foreach (var phase in phases)
                {
                    var tasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/phase/{phase.Id}");
                    phase.Tasks = tasks;

                    // Calculate completion percentage
                    if (tasks.Any())
                    {
                        phase.CompletionPercentage = (double)tasks.Count(t => t.Status == "Done") / tasks.Count * 100;
                    }
                }

                var allTasks = phases.SelectMany(p => p.Tasks).ToList();
                var totalCost = phases.Sum(p => p.PhaseCost);

                var viewModel = new TimelineViewModel
                {
                    Project = project,
                    Phases = phases,
                    TotalBudget = project.Budget,
                    TotalCost = totalCost,
                    TotalTasks = allTasks.Count,
                    CompletedTasks = allTasks.Count(t => t.Status == "Done"),
                    OverdueTasks = allTasks.Count(t => t.IsOverdue),
                    OverallProgress = allTasks.Any() ? (double)allTasks.Count(t => t.Status == "Done") / allTasks.Count * 100 : 0
                };

                ViewBag.PMId = pmId;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading timeline for project {projectId}");
                TempData["Error"] = "Failed to load timeline";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== PHASES ==========

        [HttpGet]
        public async Task<IActionResult> CreatePhase(string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projectId}");
                var existingPhases = await _apiService.GetAsync<List<PhaseViewModel>>($"api/phases/project/{projectId}");

                ViewBag.Project = project;
                ViewBag.PMId = pmId;
                ViewBag.NextOrderIndex = existingPhases.Any() ? existingPhases.Max(p => p.OrderIndex) + 1 : 1;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create phase form");
                TempData["Error"] = "Failed to load form";
                return RedirectToAction(nameof(Timeline), new { projectId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePhase(CreatePhaseViewModel model)
        {
            var pmId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _apiService.PostAsync<PhaseViewModel>("api/phases", model);
                TempData["Success"] = "Phase created successfully";
                return RedirectToAction(nameof(Timeline), new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating phase");
                TempData["Error"] = "Failed to create phase";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPhase(string id)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var phase = await _apiService.GetAsync<PhaseViewModel>($"api/phases/{id}");
                ViewBag.PMId = pmId;
                return View(phase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading phase {id}");
                TempData["Error"] = "Failed to load phase";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditPhase(string id, PhaseViewModel model)
        {
            var pmId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _apiService.PutAsync<object>($"api/phases/{id}", model);
                TempData["Success"] = "Phase updated successfully";
                return RedirectToAction(nameof(Timeline), new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating phase {id}");
                TempData["Error"] = "Failed to update phase";
                return View(model);
            }
        }

        // ========== TASKS ==========

        [HttpGet]
        public async Task<IActionResult> CreateTask(string phaseId, string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var phase = await _apiService.GetAsync<PhaseViewModel>($"api/phases/{phaseId}");
                var contractors = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                var contractorList = contractors.Where(u => u.Role == "Contractor" && u.Status == "Approved").ToList();
                var phaseTasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/phase/{phaseId}");

                ViewBag.Phase = phase;
                ViewBag.Contractors = contractorList;
                ViewBag.PhaseTasks = phaseTasks;
                ViewBag.PMId = pmId;
                ViewBag.ProjectId = projectId;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create task form");
                TempData["Error"] = "Failed to load form";
                return RedirectToAction(nameof(Timeline), new { projectId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskViewModel model)
        {
            var pmId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _apiService.PostAsync<TaskViewModel>("api/tasks", model);
                TempData["Success"] = "Task created successfully";
                return RedirectToAction(nameof(Timeline), new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                TempData["Error"] = "Failed to create task";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTask(string id)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{id}");
                var contractors = await _apiService.GetAsync<List<UserViewModel>>("api/auth/users");
                var contractorList = contractors.Where(u => u.Role == "Contractor" && u.Status == "Approved").ToList();

                ViewBag.Contractors = contractorList;
                ViewBag.PMId = pmId;
                return View(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading task {id}");
                TempData["Error"] = "Failed to load task";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTask(string id, TaskViewModel model)
        {
            var pmId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _apiService.PutAsync<object>($"api/tasks/{id}", model);
                TempData["Success"] = "Task updated successfully";
                return RedirectToAction(nameof(Timeline), new { projectId = model.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task {id}");
                TempData["Error"] = "Failed to update task";
                return View(model);
            }
        }

        // ========== QUOTATIONS ==========

        public async Task<IActionResult> Quotations(string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projectId}");
                var quotations = await _apiService.GetAsync<List<QuotationViewModel>>($"api/quotations/project/{projectId}");

                // Get task names
                foreach (var quotation in quotations)
                {
                    try
                    {
                        var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{quotation.TaskId}");
                        quotation.TaskName = task.TaskName;
                    }
                    catch
                    {
                        quotation.TaskName = "Unknown Task";
                    }
                }

                ViewBag.Project = project;
                ViewBag.PMId = pmId;
                return View(quotations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading quotations for project {projectId}");
                TempData["Error"] = "Failed to load quotations";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveQuotation(string id, string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                await _apiService.PutAsync<object>($"api/quotations/{id}/approve", new { ReviewedBy = pmId });
                TempData["Success"] = "Quotation approved successfully";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving quotation {id}");
                TempData["Error"] = "Failed to approve quotation";
            }
            return RedirectToAction(nameof(Quotations), new { projectId });
        }

        [HttpPost]
        public async Task<IActionResult> RejectQuotation(string id, string reason, string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                await _apiService.PutAsync<object>($"api/quotations/{id}/reject", new { ReviewedBy = pmId, Reason = reason });
                TempData["Success"] = "Quotation rejected";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting quotation {id}");
                TempData["Error"] = "Failed to reject quotation";
            }
            return RedirectToAction(nameof(Quotations), new { projectId });
        }

        // ========== BUDGET MANAGEMENT ==========

        public async Task<IActionResult> BudgetManagement(string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projectId}");
                var phases = await _apiService.GetAsync<List<PhaseViewModel>>($"api/phases/project/{projectId}");

                var totalAllocated = phases.Sum(p => p.PhaseBudget);
                var totalSpent = phases.Sum(p => p.PhaseCost);
                var remaining = project.Budget - totalSpent;
                var percentageSpent = project.Budget > 0 ? (totalSpent / project.Budget) * 100 : 0;

                // Generate alerts
                var alerts = new List<BudgetAlert>();
                foreach (var phase in phases)
                {
                    if (phase.PercentageUsed > 90)
                    {
                        alerts.Add(new BudgetAlert
                        {
                            Type = "Danger",
                            Message = $"Phase '{phase.PhaseName}' has exceeded 90% of budget",
                            PhaseId = phase.Id,
                            PhaseName = phase.PhaseName
                        });
                    }
                    else if (phase.PercentageUsed > 75)
                    {
                        alerts.Add(new BudgetAlert
                        {
                            Type = "Warning",
                            Message = $"Phase '{phase.PhaseName}' has used over 75% of budget",
                            PhaseId = phase.Id,
                            PhaseName = phase.PhaseName
                        });
                    }
                }

                var viewModel = new BudgetSummaryViewModel
                {
                    Project = project,
                    TotalProjectBudget = project.Budget,
                    TotalAllocatedBudget = totalAllocated,
                    TotalSpent = totalSpent,
                    RemainingBudget = remaining,
                    PercentageSpent = percentageSpent,
                    Phases = phases,
                    Alerts = alerts
                };

                ViewBag.PMId = pmId;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading budget management for project {projectId}");
                TempData["Error"] = "Failed to load budget information";
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== REPORTS ==========

        public async Task<IActionResult> PhaseReport(string phaseId, string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var phase = await _apiService.GetAsync<PhaseViewModel>($"api/phases/{phaseId}");
                var tasks = await _apiService.GetAsync<List<TaskViewModel>>($"api/tasks/phase/{phaseId}");

                var completedTasks = tasks.Where(t => t.Status == "Done").ToList();
                var delayedTasks = completedTasks.Where(t => t.DaysLate > 0).ToList();
                var onTimeTasks = completedTasks.Where(t => t.DaysLate == 0).ToList();

                var viewModel = new PhaseReportViewModel
                {
                    Phase = phase,
                    Tasks = tasks,
                    TotalTasks = tasks.Count,
                    CompletedTasks = completedTasks.Count,
                    DelayedTasks = delayedTasks.Count,
                    OnTimeTasks = onTimeTasks.Count,
                    AverageDelayDays = delayedTasks.Any() ? delayedTasks.Average(t => t.DaysLate) : 0,
                    TotalEstimatedDays = tasks.Sum(t => (t.EstimatedEndDate - t.EstimatedStartDate).Days),
                    TotalActualDays = completedTasks.Where(t => t.ActualEndDate.HasValue && t.ActualStartDate.HasValue)
                                                    .Sum(t => (t.ActualEndDate.Value - t.ActualStartDate.Value).Days),
                    PhaseStartDate = tasks.Any() ? tasks.Min(t => t.EstimatedStartDate) : null,
                    PhaseEndDate = tasks.Any() ? tasks.Max(t => t.EstimatedEndDate) : null
                };

                ViewBag.PMId = pmId;
                ViewBag.ProjectId = projectId;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading phase report {phaseId}");
                TempData["Error"] = "Failed to load report";
                return RedirectToAction(nameof(Timeline), new { projectId });
            }
        }

        // ========== CONTRACTOR SURVEY ==========

        [HttpGet]
        public async Task<IActionResult> CreateSurvey(string taskId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{taskId}");

                // Check if survey already exists
                try
                {
                    var existingSurvey = await _apiService.GetAsync<ContractorSurveyViewModel>($"api/surveys/task/{taskId}");
                    if (existingSurvey != null)
                    {
                        TempData["Error"] = "A survey already exists for this task";
                        return RedirectToAction(nameof(Timeline), new { projectId = task.ProjectId });
                    }
                }
                catch { }

                var contractor = await _apiService.GetAsync<UserViewModel>($"api/auth/user/{task.AssignedContractorId}");

                var viewModel = new ContractorSurveyViewModel
                {
                    TaskId = taskId,
                    TaskName = task.TaskName,
                    ContractorId = task.AssignedContractorId,
                    ContractorName = contractor.Name,
                    ProjectManagerId = pmId
                };

                ViewBag.Task = task;
                ViewBag.PMId = pmId;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading survey form for task {taskId}");
                TempData["Error"] = "Failed to load survey form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSurvey(ContractorSurveyViewModel model)
        {
            var pmId = GetCurrentUserId();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var task = await _apiService.GetAsync<TaskViewModel>($"api/tasks/{model.TaskId}");

                var survey = new
                {
                    TaskId = model.TaskId,
                    ContractorId = model.ContractorId,
                    ProjectId = task.ProjectId,
                    ProjectManagerId = pmId,
                    TimelinessRating = model.TimelinessRating,
                    QualityRating = model.QualityRating,
                    CommunicationRating = model.CommunicationRating,
                    ProfessionalismRating = model.ProfessionalismRating,
                    Comments = model.Comments,
                    WouldRecommend = model.WouldRecommend
                };

                await _apiService.PostAsync<object>("api/surveys", survey);
                TempData["Success"] = "Survey submitted successfully";
                return RedirectToAction(nameof(Timeline), new { projectId = task.ProjectId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating survey");
                TempData["Error"] = "Failed to submit survey";
                return View(model);
            }
        }

        // ========== PROJECT DOCUMENTS ==========

        public async Task<IActionResult> ProjectDocuments(string projectId)
        {
            var pmId = GetCurrentUserId();

            try
            {
                var project = await _apiService.GetAsync<ProjectViewModel>($"api/projects/{projectId}");
                var documents = await _apiService.GetAsync<List<DocumentViewModel>>($"api/documents/project/{projectId}");

                ViewBag.Project = project;
                ViewBag.PMId = pmId;
                return View(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading documents for project {projectId}");
                TempData["Error"] = "Failed to load documents";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}