using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IStorageService _storageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            IFirebaseService firebaseService,
            IStorageService storageService,
            ILogger<MessagesController> logger)
        {
            _firebaseService = firebaseService;
            _storageService = storageService;
            _logger = logger;
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<Message>>> GetProjectMessages(string projectId)
        {
            try
            {
                var messages = await _firebaseService.QueryCollectionAsync<Message>("messages", "ProjectId", projectId);
                return Ok(messages.OrderBy(m => m.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving messages for project {projectId}");
                return StatusCode(500, new { error = "Failed to retrieve messages" });
            }
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<Message>> SendMessage(
    [FromForm] string projectId,
    [FromForm] string senderId,
    [FromForm] string senderName,
    [FromForm] string messageText,
    [FromForm] IFormFile attachment = null)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(projectId))
                    return BadRequest(new { error = "ProjectId is required" });

                if (string.IsNullOrEmpty(senderId))
                    return BadRequest(new { error = "SenderId is required" });

                if (string.IsNullOrEmpty(senderName))
                    return BadRequest(new { error = "SenderName is required" });

                var message = new Message
                {
                    ProjectId = projectId,
                    SenderId = senderId,
                    SenderName = senderName,
                    MessageText = messageText ?? "",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                // Upload attachment if provided
                if (attachment != null && attachment.Length > 0)
                {
                    var filePath = await _storageService.UploadFileAsync(attachment, "attachments");
                    message.AttachmentUrl = _storageService.GetFileUrl(filePath);
                }

                var messageId = await _firebaseService.AddDocumentAsync("messages", message);
                message.Id = messageId;

                // Log audit
                await LogAudit(senderId, senderName, "Send Message", "Message", messageId, $"Sent message in project {projectId}");

                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { error = "Failed to send message", details = ex.Message });
            }
        }
        [HttpGet]
        public async Task<ActionResult<List<Message>>> GetAllMessages()
        {
            try
            {
                var messages = await _firebaseService.GetCollectionAsync<Message>("messages");
                return Ok(messages.OrderByDescending(m => m.CreatedAt).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all messages");
                return StatusCode(500, new { error = "Failed to retrieve messages" });
            }
        }

        private async Task LogAudit(string userId, string userName, string action, string entityType, string entityId, string details)
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow
            };
            await _firebaseService.AddDocumentAsync("audit_logs", log);
        }
    }

  
}