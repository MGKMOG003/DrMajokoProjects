using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationsController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<QuotationsController> _logger;

        public QuotationsController(IFirebaseService firebaseService, ILogger<QuotationsController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        // Get all quotations for a project
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<Quotation>>> GetProjectQuotations(string projectId)
        {
            try
            {
                var quotations = await _firebaseService.QueryCollectionAsync<Quotation>("quotations", "ProjectId", projectId);
                return Ok(quotations.OrderByDescending(q => q.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotations for project {projectId}");
                return StatusCode(500, new { error = "Failed to retrieve quotations" });
            }
        }

        // Get quotations for a specific task
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<List<Quotation>>> GetTaskQuotations(string taskId)
        {
            try
            {
                var quotations = await _firebaseService.QueryCollectionAsync<Quotation>("quotations", "TaskId", taskId);
                return Ok(quotations.OrderByDescending(q => q.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotations for task {taskId}");
                return StatusCode(500, new { error = "Failed to retrieve quotations" });
            }
        }

        // Get quotations by contractor
        [HttpGet("contractor/{contractorId}")]
        public async Task<ActionResult<List<Quotation>>> GetContractorQuotations(string contractorId)
        {
            try
            {
                var quotations = await _firebaseService.QueryCollectionAsync<Quotation>("quotations", "ContractorId", contractorId);
                return Ok(quotations.OrderByDescending(q => q.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotations for contractor {contractorId}");
                return StatusCode(500, new { error = "Failed to retrieve quotations" });
            }
        }

        // Get single quotation
        [HttpGet("{id}")]
        public async Task<ActionResult<Quotation>> GetQuotation(string id)
        {
            try
            {
                var quotation = await _firebaseService.GetDocumentAsync<Quotation>("quotations", id);
                if (quotation == null)
                    return NotFound(new { error = "Quotation not found" });

                return Ok(quotation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving quotation {id}");
                return StatusCode(500, new { error = "Failed to retrieve quotation" });
            }
        }

        // Create new quotation
        [HttpPost]
        public async Task<ActionResult<Quotation>> CreateQuotation([FromBody] Quotation quotation)
        {
            try
            {
                quotation.CreatedAt = DateTime.UtcNow;
                quotation.UpdatedAt = DateTime.UtcNow;

                // Auto-generate quotation number if not provided
                if (string.IsNullOrEmpty(quotation.QuotationNumber))
                {
                    quotation.QuotationNumber = $"QT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                }

                // Set status based on type
                if (quotation.QuotationType == "Task")
                {
                    quotation.Status = "Submitted";
                    quotation.SubmittedAt = DateTime.UtcNow;

                    // Calculate total from material + labor
                    quotation.TotalAmount = quotation.MaterialCost + quotation.LaborCost;
                }
                else
                {
                    // General quotation: calculate from items
                    if (quotation.Items != null && quotation.Items.Any())
                    {
                        quotation.SubTotal = quotation.Items.Sum(item => item.Total);
                        quotation.TotalAmount = quotation.SubTotal + quotation.Tax;
                    }

                    if (string.IsNullOrEmpty(quotation.Status))
                    {
                        quotation.Status = "Draft";
                    }
                }

                var quotationId = await _firebaseService.AddDocumentAsync("quotations", quotation);
                quotation.Id = quotationId;

                return CreatedAtAction(nameof(GetQuotation), new { id = quotationId }, quotation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quotation");
                return StatusCode(500, new { error = "Failed to create quotation" });
            }
        }

        // Update quotation
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateQuotation(string id, [FromBody] Quotation quotation)
        {
            try
            {
                quotation.Id = id;
                quotation.UpdatedAt = DateTime.UtcNow;
                await _firebaseService.UpdateDocumentAsync("quotations", id, quotation);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating quotation {id}");
                return StatusCode(500, new { error = "Failed to update quotation" });
            }
        }

        // Approve quotation (PM action)
        [HttpPut("{id}/approve")]
        public async Task<ActionResult> ApproveQuotation(string id, [FromBody] ApproveQuotationRequest request)
        {
            try
            {
                var quotation = await _firebaseService.GetDocumentAsync<Quotation>("quotations", id);
                if (quotation == null)
                    return NotFound(new { error = "Quotation not found" });

                quotation.Status = "Approved";
                quotation.ReviewedAt = DateTime.UtcNow;
                quotation.ReviewedBy = request.ReviewedBy;
                quotation.UpdatedAt = DateTime.UtcNow;

                await _firebaseService.UpdateDocumentAsync("quotations", id, quotation);

                // If it's a task quotation, update task cost
                if (!string.IsNullOrEmpty(quotation.TaskId))
                {
                    var task = await _firebaseService.GetDocumentAsync<ProjectTask>("tasks", quotation.TaskId);
                    if (task != null)
                    {
                        task.EstimatedCost = quotation.TotalAmount;
                        task.ActualCost = quotation.TotalAmount;
                        task.UpdatedAt = DateTime.UtcNow;
                        await _firebaseService.UpdateDocumentAsync("tasks", task.Id, task);

                        // Update phase cost
                        var phase = await _firebaseService.GetDocumentAsync<Phase>("phases", task.PhaseId);
                        if (phase != null)
                        {
                            phase.PhaseCost += quotation.TotalAmount;
                            phase.UpdatedAt = DateTime.UtcNow;
                            await _firebaseService.UpdateDocumentAsync("phases", phase.Id, phase);
                        }
                    }
                }

                return Ok(new { message = "Quotation approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving quotation {id}");
                return StatusCode(500, new { error = "Failed to approve quotation" });
            }
        }

        // Reject quotation (PM action)
        [HttpPut("{id}/reject")]
        public async Task<ActionResult> RejectQuotation(string id, [FromBody] RejectQuotationRequest request)
        {
            try
            {
                var quotation = await _firebaseService.GetDocumentAsync<Quotation>("quotations", id);
                if (quotation == null)
                    return NotFound(new { error = "Quotation not found" });

                quotation.Status = "Rejected";
                quotation.ReviewedAt = DateTime.UtcNow;
                quotation.ReviewedBy = request.ReviewedBy;
                quotation.RejectionReason = request.Reason;
                quotation.UpdatedAt = DateTime.UtcNow;

                await _firebaseService.UpdateDocumentAsync("quotations", id, quotation);

                return Ok(new { message = "Quotation rejected successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting quotation {id}");
                return StatusCode(500, new { error = "Failed to reject quotation" });
            }
        }

        // Delete quotation
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteQuotation(string id)
        {
            try
            {
                await _firebaseService.DeleteDocumentAsync("quotations", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting quotation {id}");
                return StatusCode(500, new { error = "Failed to delete quotation" });
            }
        }
    }

    public class ApproveQuotationRequest
    {
        public string ReviewedBy { get; set; }
    }

    public class RejectQuotationRequest
    {
        public string ReviewedBy { get; set; }
        public string Reason { get; set; }
    }
}