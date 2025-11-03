using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IStorageService _storageService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            IFirebaseService firebaseService,
            IStorageService storageService,
            ILogger<TasksController> logger)
        {
            _firebaseService = firebaseService;
            _storageService = storageService;
            _logger = logger;
        }

        [HttpGet("phase/{phaseId}")]
        public async Task<ActionResult<List<ProjectTask>>> GetPhaseTasks(string phaseId)
        {
            try
            {
                var tasks = await _firebaseService.QueryCollectionAsync<ProjectTask>("tasks", "PhaseId", phaseId);
                return Ok(tasks.OrderBy(t => t.EstimatedStartDate).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks for phase {phaseId}");
                return StatusCode(500, new { error = "Failed to retrieve tasks" });
            }
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<ProjectTask>>> GetProjectTasks(string projectId)
        {
            try
            {
                var tasks = await _firebaseService.QueryCollectionAsync<ProjectTask>("tasks", "ProjectId", projectId);
                return Ok(tasks.OrderBy(t => t.EstimatedStartDate).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks for project {projectId}");
                return StatusCode(500, new { error = "Failed to retrieve tasks" });
            }
        }

        [HttpGet("contractor/{contractorId}")]
        public async Task<ActionResult<List<ProjectTask>>> GetContractorTasks(string contractorId)
        {
            try
            {
                var tasks = await _firebaseService.QueryCollectionAsync<ProjectTask>("tasks", "AssignedContractorId", contractorId);
                return Ok(tasks.OrderBy(t => t.EstimatedStartDate).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks for contractor {contractorId}");
                return StatusCode(500, new { error = "Failed to retrieve tasks" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectTask>> GetTask(string id)
        {
            try
            {
                var task = await _firebaseService.GetDocumentAsync<ProjectTask>("tasks", id);
                if (task == null)
                    return NotFound(new { error = "Task not found" });

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving task {id}");
                return StatusCode(500, new { error = "Failed to retrieve task" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProjectTask>> CreateTask([FromBody] ProjectTask task)
        {
            try
            {
                task.CreatedAt = DateTime.UtcNow;
                task.UpdatedAt = DateTime.UtcNow;
                task.ActualCost = 0;
                task.CompletionPhotoUrls = new List<string>();

                var taskId = await _firebaseService.AddDocumentAsync("tasks", task);
                task.Id = taskId;

                return CreatedAtAction(nameof(GetTask), new { id = taskId }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, new { error = "Failed to create task" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(string id, [FromBody] ProjectTask task)
        {
            try
            {
                task.Id = id;
                task.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("tasks", id, task);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task {id}");
                return StatusCode(500, new { error = "Failed to update task" });
            }
        }

        // NEW: Update task status (for contractors)
        [HttpPut("{id}/status")]
        public async Task<ActionResult> UpdateTaskStatus(string id, [FromBody] UpdateTaskStatusRequest request)
        {
            try
            {
                var task = await _firebaseService.GetDocumentAsync<ProjectTask>("tasks", id);
                if (task == null)
                    return NotFound(new { error = "Task not found" });

                task.Status = request.Status;
                task.UpdatedAt = DateTime.UtcNow;

                // Set actual start date when task starts
                if (request.Status == "In Progress" && !task.ActualStartDate.HasValue)
                {
                    task.ActualStartDate = DateTime.UtcNow;
                }
                // Set actual end date and cost when task completes
                else if (request.Status == "Done")
                {
                    task.ActualEndDate = DateTime.UtcNow;
                    if (request.ActualCost.HasValue)
                    {
                        task.ActualCost = request.ActualCost.Value;
                    }
                }

                if (!string.IsNullOrEmpty(request.CompletionNotes))
                {
                    task.CompletionNotes = request.CompletionNotes;
                }

                await _firebaseService.UpdateDocumentAsync("tasks", id, task);

                return Ok(new { message = "Task status updated successfully", task });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task status {id}");
                return StatusCode(500, new { error = "Failed to update task status" });
            }
        }

        // NEW: Upload completion photos (for contractors)
        [HttpPost("{id}/completion-photos")]
        public async Task<ActionResult> UploadCompletionPhotos(string id, [FromForm] List<IFormFile> photos)
        {
            try
            {
                var task = await _firebaseService.GetDocumentAsync<ProjectTask>("tasks", id);
                if (task == null)
                    return NotFound(new { error = "Task not found" });

                if (task.CompletionPhotoUrls == null)
                    task.CompletionPhotoUrls = new List<string>();

                foreach (var photo in photos)
                {
                    if (photo.Length > 0)
                    {
                        var filePath = await _storageService.UploadFileAsync(photo, $"tasks/{id}/completion");
                        var photoUrl = _storageService.GetFileUrl(filePath);
                        task.CompletionPhotoUrls.Add(photoUrl);
                    }
                }

                task.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("tasks", id, task);

                return Ok(new { message = "Photos uploaded successfully", photoUrls = task.CompletionPhotoUrls });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading completion photos for task {id}");
                return StatusCode(500, new { error = "Failed to upload photos" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(string id)
        {
            try
            {
                await _firebaseService.DeleteDocumentAsync("tasks", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task {id}");
                return StatusCode(500, new { error = "Failed to delete task" });
            }
        }
    }

    // Request DTOs
    public class UpdateTaskStatusRequest
    {
        public string Status { get; set; }
        public double? ActualCost { get; set; }
        public string CompletionNotes { get; set; }
    }
}