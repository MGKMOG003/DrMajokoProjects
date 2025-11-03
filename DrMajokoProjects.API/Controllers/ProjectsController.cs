using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;
using DrMajokoProjects.API.Models.DTOs;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(IFirebaseService firebaseService, ILogger<ProjectsController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Project>>> GetAllProjects()
        {
            try
            {
                var projects = await _firebaseService.GetCollectionAsync<Project>("projects");
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects");
                return StatusCode(500, new { error = "Failed to retrieve projects" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(string id)
        {
            try
            {
                var project = await _firebaseService.GetDocumentAsync<Project>("projects", id);
                if (project == null)
                    return NotFound(new { error = "Project not found" });

                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving project {id}");
                return StatusCode(500, new { error = "Failed to retrieve project" });
            }
        }

        // GET projects by Project Manager
        [HttpGet("pm/{pmId}")]
        public async Task<ActionResult<List<Project>>> GetProjectsByPM(string pmId)
        {
            try
            {
                var projects = await _firebaseService.QueryCollectionAsync<Project>("projects", "ProjectManagerId", pmId);
                return Ok(projects.OrderByDescending(p => p.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving projects for PM {pmId}");
                return StatusCode(500, new { error = "Failed to retrieve projects" });
            }
        }

        // GET projects by Client
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<List<Project>>> GetProjectsByClient(string clientId)
        {
            try
            {
                var projects = await _firebaseService.QueryCollectionAsync<Project>("projects", "ClientId", clientId);
                return Ok(projects.OrderByDescending(p => p.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving projects for client {clientId}");
                return StatusCode(500, new { error = "Failed to retrieve projects" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                var project = new Project
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ClientId = dto.ClientId,
                    ProjectManagerId = dto.ProjectManagerId,
                    Budget = dto.Budget,
                    ActualSpent = 0,
                    Status = "Planning",
                    StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
                    EndDate = dto.EndDate.HasValue ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) : (DateTime?)null,
                    Location = dto.Location,
                    Phases = new List<ProjectPhase>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var projectId = await _firebaseService.AddDocumentAsync("projects", project);
                project.Id = projectId;

                return CreatedAtAction(nameof(GetProject), new { id = projectId }, project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { error = "Failed to create project", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProject(string id, [FromBody] Project project)
        {
            try
            {
                project.Id = id;
                project.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("projects", id, project);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project {id}");
                return StatusCode(500, new { error = "Failed to update project" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProject(string id)
        {
            try
            {
                await _firebaseService.DeleteDocumentAsync("projects", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting project {id}");
                return StatusCode(500, new { error = "Failed to delete project" });
            }
        }

        // Assign Project Manager to Project
        [HttpPut("{id}/assign-pm")]
        public async Task<ActionResult> AssignProjectManager(string id, [FromBody] AssignPMRequest request)
        {
            try
            {
                var project = await _firebaseService.GetDocumentAsync<Project>("projects", id);
                if (project == null)
                    return NotFound(new { error = "Project not found" });

                project.ProjectManagerId = request.ProjectManagerId;
                project.UpdatedAt = DateTime.UtcNow;

                await _firebaseService.UpdateDocumentAsync("projects", id, project);

                return Ok(new { message = "Project Manager assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning PM to project {id}");
                return StatusCode(500, new { error = "Failed to assign Project Manager" });
            }
        }
    }

    // Request model for assigning PM
    public class AssignPMRequest
    {
        public string ProjectManagerId { get; set; }
    }
}