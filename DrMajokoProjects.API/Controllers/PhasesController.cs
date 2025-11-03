using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhasesController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<PhasesController> _logger;

        public PhasesController(IFirebaseService firebaseService, ILogger<PhasesController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<Phase>>> GetProjectPhases(string projectId)
        {
            try
            {
                var phases = await _firebaseService.QueryCollectionAsync<Phase>("phases", "ProjectId", projectId);
                return Ok(phases.OrderBy(p => p.OrderIndex).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving phases for project {projectId}");
                return StatusCode(500, new { error = "Failed to retrieve phases" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Phase>> GetPhase(string id)
        {
            try
            {
                var phase = await _firebaseService.GetDocumentAsync<Phase>("phases", id);
                if (phase == null)
                    return NotFound(new { error = "Phase not found" });

                return Ok(phase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving phase {id}");
                return StatusCode(500, new { error = "Failed to retrieve phase" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Phase>> CreatePhase([FromBody] CreatePhaseDto dto)
        {
            try
            {
                var phase = new Phase
                {
                    ProjectId = dto.ProjectId,
                    PhaseName = dto.PhaseName,
                    PhaseBudget = dto.PhaseBudget,
                    PhaseCost = 0,
                    EstimatedStartDate = DateTime.SpecifyKind(dto.EstimatedStartDate, DateTimeKind.Utc),
                    EstimatedEndDate = DateTime.SpecifyKind(dto.EstimatedEndDate, DateTimeKind.Utc),
                    Status = "Pending",
                    OrderIndex = dto.OrderIndex,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var phaseId = await _firebaseService.AddDocumentAsync("phases", phase);
                phase.Id = phaseId;

                return CreatedAtAction(nameof(GetPhase), new { id = phaseId }, phase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating phase");
                return StatusCode(500, new { error = "Failed to create phase", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePhase(string id, [FromBody] Phase phase)
        {
            try
            {
                phase.Id = id;
                phase.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("phases", id, phase);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating phase {id}");
                return StatusCode(500, new { error = "Failed to update phase" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePhase(string id)
        {
            try
            {
                await _firebaseService.DeleteDocumentAsync("phases", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting phase {id}");
                return StatusCode(500, new { error = "Failed to delete phase" });
            }
        }
    }

    // DTO for creating phases
    public class CreatePhaseDto
    {
        public string ProjectId { get; set; }
        public string PhaseName { get; set; }
        public double PhaseBudget { get; set; }
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public int OrderIndex { get; set; }
    }
}