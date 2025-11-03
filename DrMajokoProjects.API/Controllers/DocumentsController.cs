using Microsoft.AspNetCore.Mvc;
using DrMajokoProjects.API.Services.Interfaces;
using DrMajokoProjects.API.Models.Entities;

namespace DrMajokoProjects.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IFirebaseService _firebaseService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IStorageService storageService,
            IFirebaseService firebaseService,
            ILogger<DocumentsController> logger)
        {
            _storageService = storageService;
            _firebaseService = firebaseService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<ActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                    return BadRequest(new { error = "No file provided" });

                // Determine folder based on file type
                string folder = request.FileType?.ToLower() switch
                {
                    "blueprint" => "blueprints",
                    "photo" => "photos",
                    "report" => "reports",
                    _ => "documents"
                };

                // Upload file
                var filePath = await _storageService.UploadFileAsync(request.File, folder);
                var fileUrl = _storageService.GetFileUrl(filePath);

                // Save metadata to Firestore
                var document = new Document
                {
                    ProjectId = request.ProjectId,
                    FileName = request.File.FileName,
                    FileType = request.FileType,
                    FilePath = filePath,
                    FileUrl = fileUrl,
                    FileSize = request.File.Length,
                    UploadedBy = request.UploadedBy,
                    UploadedAt = DateTime.UtcNow
                };

                var docId = await _firebaseService.AddDocumentAsync("documents", document);
                document.Id = docId;

                return Ok(new { id = docId, url = fileUrl, document = document });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new { error = "Failed to upload document" });
            }
        }

        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<List<Document>>> GetProjectDocuments(string projectId)
        {
            try
            {
                var documents = await _firebaseService.QueryCollectionAsync<Document>("documents", "ProjectId", projectId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving documents for project {projectId}");
                return StatusCode(500, new { error = "Failed to retrieve documents" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDocument(string id)
        {
            try
            {
                var document = await _firebaseService.GetDocumentAsync<Document>("documents", id);
                if (document == null)
                    return NotFound(new { error = "Document not found" });

                // Delete physical file
                await _storageService.DeleteFileAsync(document.FilePath);

                // Delete metadata
                await _firebaseService.DeleteDocumentAsync("documents", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting document {id}");
                return StatusCode(500, new { error = "Failed to delete document" });
            }
        }
    }

    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; }
        public string ProjectId { get; set; }
        public string FileType { get; set; }
        public string UploadedBy { get; set; }
    }
}