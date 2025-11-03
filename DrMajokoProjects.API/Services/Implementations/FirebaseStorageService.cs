using Google.Cloud.Storage.V1;
using DrMajokoProjects.API.Services.Interfaces;
using Google.Apis.Auth.OAuth2;

namespace DrMajokoProjects.API.Services.Implementations
{
    public class FirebaseStorageService : IStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FirebaseStorageService> _logger;

        public FirebaseStorageService(IConfiguration configuration, ILogger<FirebaseStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _bucketName = configuration["Firebase:StorageBucket"];

            try
            {
                // Get the path to credentials file
                var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-credentials.json");

                if (!File.Exists(credentialsPath))
                {
                    throw new FileNotFoundException($"Firebase credentials file not found at: {credentialsPath}");
                }

                // Create Storage client with credentials
                var credential = GoogleCredential.FromFile(credentialsPath);
                _storageClient = StorageClient.Create(credential);

                _logger.LogInformation("Firebase Storage initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Storage");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            try
            {
                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{folder}/{Guid.NewGuid()}{fileExtension}";

                // Upload to Firebase Storage
                using (var stream = file.OpenReadStream())
                {
                    await _storageClient.UploadObjectAsync(
                        bucket: _bucketName,
                        objectName: fileName,
                        contentType: file.ContentType,
                        source: stream
                    );
                }

                _logger.LogInformation($"File uploaded to Firebase Storage: {fileName}");

                // Return the file path (not full URL, we'll generate that in GetFileUrl)
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Firebase Storage");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                await _storageClient.DeleteObjectAsync(_bucketName, filePath);
                _logger.LogInformation($"File deleted from Firebase Storage: {filePath}");
                return true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"File not found in Firebase Storage: {filePath}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file from Firebase Storage: {filePath}");
                return false;
            }
        }

        public string GetFileUrl(string filePath)
        {
            // Generate public download URL
            // Format: https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{filePath}?alt=media
            var encodedPath = Uri.EscapeDataString(filePath);
            return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{encodedPath}?alt=media";
        }

        /// <summary>
        /// Get a signed URL that expires (more secure for private files)
        /// </summary>
        public async Task<string> GetSignedUrlAsync(string filePath, TimeSpan expiration)
        {
            try
            {
                var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-credentials.json");
                var credential = GoogleCredential.FromFile(credentialsPath);

                var urlSigner = UrlSigner.FromCredential(credential);

                var signedUrl = await urlSigner.SignAsync(
                    bucket: _bucketName,
                    objectName: filePath,
                    duration: expiration,
                    HttpMethod.Get
                );

                return signedUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating signed URL");
                throw;
            }
        }
    }
}