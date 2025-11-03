using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;
using DrMajokoProjects.API.Models.DTOs;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IStorageService _storageService;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(
            IFirebaseService firebaseService,
            IStorageService storageService,
            ILogger<MaintenanceController> logger)
        {
            _firebaseService = firebaseService;
            _storageService = storageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<MaintenanceRequest>>> GetAllRequests()
        {
            try
            {
                var requests = await _firebaseService.GetCollectionAsync<MaintenanceRequest>("maintenance_requests");
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving maintenance requests");
                return StatusCode(500, new { error = "Failed to retrieve maintenance requests" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRequest>> GetRequest(string id)
        {
            try
            {
                var request = await _firebaseService.GetDocumentAsync<MaintenanceRequest>("maintenance_requests", id);
                if (request == null)
                    return NotFound(new { error = "Maintenance request not found" });

                return Ok(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving maintenance request {id}");
                return StatusCode(500, new { error = "Failed to retrieve maintenance request" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<MaintenanceRequest>> CreateRequest([FromForm] CreateMaintenanceRequestDto dto, [FromForm] List<IFormFile> photos)
        {
            try
            {
                var request = new MaintenanceRequest
                {
                    ProjectId = dto.ProjectId,
                    ClientId = dto.ClientId,
                    Title = dto.Title,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = "Pending",
                    PhotoUrls = new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };

                // Upload photos if provided
                if (photos != null && photos.Any())
                {
                    foreach (var photo in photos)
                    {
                        var photoPath = await _storageService.UploadFileAsync(photo, "photos");
                        var photoUrl = _storageService.GetFileUrl(photoPath);
                        request.PhotoUrls.Add(photoUrl);
                    }
                }

                var requestId = await _firebaseService.AddDocumentAsync("maintenance_requests", request);
                request.Id = requestId;

                return CreatedAtAction(nameof(GetRequest), new { id = requestId }, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance request");
                return StatusCode(500, new { error = "Failed to create maintenance request" });
            }
        }

        [HttpPut("{id}/assign")]
        public async Task<ActionResult> AssignContractor(string id, [FromBody] string contractorId)
        {
            try
            {
                var request = await _firebaseService.GetDocumentAsync<MaintenanceRequest>("maintenance_requests", id);
                if (request == null)
                    return NotFound(new { error = "Maintenance request not found" });

                request.AssignedContractorId = contractorId;
                request.Status = "Assigned";
                request.AssignedAt = DateTime.UtcNow;

                await _firebaseService.UpdateDocumentAsync("maintenance_requests", id, request);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning contractor to maintenance request {id}");
                return StatusCode(500, new { error = "Failed to assign contractor" });
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult> CompleteRequest(string id, [FromBody] string completionNotes)
        {
            try
            {
                var request = await _firebaseService.GetDocumentAsync<MaintenanceRequest>("maintenance_requests", id);
                if (request == null)
                    return NotFound(new { error = "Maintenance request not found" });

                request.Status = "Completed";
                request.CompletedAt = DateTime.UtcNow;
                request.CompletionNotes = completionNotes;

                await _firebaseService.UpdateDocumentAsync("maintenance_requests", id, request);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing maintenance request {id}");
                return StatusCode(500, new { error = "Failed to complete maintenance request" });
            }
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<MaintenanceRequest>>> GetRequestsByProject(string projectId)
        {
            try
            {
                var requests = await _firebaseService.QueryCollectionAsync<MaintenanceRequest>("maintenance_requests", "ProjectId", projectId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving maintenance requests for project {projectId}");
                return StatusCode(500, new { error = "Failed to retrieve maintenance requests" });
            }
        }
    }
}