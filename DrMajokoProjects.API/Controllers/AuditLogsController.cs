using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(IFirebaseService firebaseService, ILogger<AuditLogsController> logger)
        {
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuditLog>>> GetAllLogs()
        {
            try
            {
                var logs = await _firebaseService.GetCollectionAsync<AuditLog>("audit_logs");
                return Ok(logs.OrderByDescending(l => l.Timestamp).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, new { error = "Failed to retrieve audit logs" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<AuditLog>>> GetUserLogs(string userId)
        {
            try
            {
                var logs = await _firebaseService.QueryCollectionAsync<AuditLog>("audit_logs", "UserId", userId);
                return Ok(logs.OrderByDescending(l => l.Timestamp).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving logs for user {userId}");
                return StatusCode(500, new { error = "Failed to retrieve user logs" });
            }
        }

        [HttpPost]
        public async Task<ActionResult> LogAction([FromBody] AuditLog log)
        {
            try
            {
                log.Timestamp = DateTime.UtcNow;
                await _firebaseService.AddDocumentAsync("audit_logs", log);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log");
                return StatusCode(500, new { error = "Failed to create audit log" });
            }
        }
    }
}