using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractorsController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<ContractorsController> _logger;

        public ContractorsController(IFirebaseService firebaseService, ILogger<ContractorsController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Contractor>>> GetAllContractors()
        {
            try
            {
                var contractors = await _firebaseService.GetCollectionAsync<Contractor>("contractors");
                return Ok(contractors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contractors");
                return StatusCode(500, new { error = "Failed to retrieve contractors" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contractor>> GetContractor(string id)
        {
            try
            {
                var contractor = await _firebaseService.GetDocumentAsync<Contractor>("contractors", id);
                if (contractor == null)
                    return NotFound(new { error = "Contractor not found" });

                return Ok(contractor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving contractor {id}");
                return StatusCode(500, new { error = "Failed to retrieve contractor" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Contractor>> CreateContractor([FromBody] Contractor contractor)
        {
            try
            {
                contractor.CreatedAt = DateTime.UtcNow;
                contractor.IsAvailable = true;
                contractor.CompletedTasks = 0;
                contractor.Rating = 0;

                var contractorId = await _firebaseService.AddDocumentAsync("contractors", contractor);
                contractor.Id = contractorId;

                return CreatedAtAction(nameof(GetContractor), new { id = contractorId }, contractor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contractor");
                return StatusCode(500, new { error = "Failed to create contractor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateContractor(string id, [FromBody] Contractor contractor)
        {
            try
            {
                contractor.Id = id;
                await _firebaseService.UpdateDocumentAsync("contractors", id, contractor);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating contractor {id}");
                return StatusCode(500, new { error = "Failed to update contractor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteContractor(string id)
        {
            try
            {
                await _firebaseService.DeleteDocumentAsync("contractors", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting contractor {id}");
                return StatusCode(500, new { error = "Failed to delete contractor" });
            }
        }
    }
}