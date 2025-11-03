using Google.Cloud.Firestore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using DrMajokoProjects.API.Services.Interfaces;

namespace DrMajokoProjects.API.Services.Implementations
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(IConfiguration configuration, ILogger<FirebaseService> logger)
        {
            _logger = logger;

            try
            {
                var projectId = configuration["Firebase:ProjectId"];

                // Get the path to credentials file
                var credentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase-credentials.json");

                if (!File.Exists(credentialsPath))
                {
                    throw new FileNotFoundException($"Firebase credentials file not found at: {credentialsPath}");
                }

                // Set environment variable for Google credentials
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath),
                        ProjectId = projectId
                    });
                }

                _firestoreDb = FirestoreDb.Create(projectId);
                _logger.LogInformation("Firebase initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase");
                throw;
            }
        }

        public FirestoreDb GetFirestoreDb() => _firestoreDb;

        public async Task<T> GetDocumentAsync<T>(string collection, string documentId) where T : class
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection(collection).Document(documentId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                    return null;

                var data = snapshot.ConvertTo<T>();

                // Set Id property if it exists
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null)
                {
                    idProperty.SetValue(data, snapshot.Id);
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting document {documentId} from {collection}");
                throw;
            }
        }

        public async Task<string> AddDocumentAsync<T>(string collection, T data) where T : class
        {
            try
            {
                CollectionReference colRef = _firestoreDb.Collection(collection);
                DocumentReference docRef = await colRef.AddAsync(data);
                _logger.LogInformation($"Document added to {collection} with ID: {docRef.Id}");
                return docRef.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding document to {collection}");
                throw;
            }
        }

        public async Task UpdateDocumentAsync<T>(string collection, string documentId, T data) where T : class
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection(collection).Document(documentId);
                await docRef.SetAsync(data, SetOptions.MergeAll);
                _logger.LogInformation($"Document {documentId} updated in {collection}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating document {documentId} in {collection}");
                throw;
            }
        }

        public async Task DeleteDocumentAsync(string collection, string documentId)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection(collection).Document(documentId);
                await docRef.DeleteAsync();
                _logger.LogInformation($"Document {documentId} deleted from {collection}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting document {documentId} from {collection}");
                throw;
            }
        }

        public async Task<List<T>> GetCollectionAsync<T>(string collection) where T : class
        {
            try
            {
                QuerySnapshot snapshot = await _firestoreDb.Collection(collection).GetSnapshotAsync();
                var results = new List<T>();

                foreach (var document in snapshot.Documents)
                {
                    var data = document.ConvertTo<T>();

                    // Set Id property if it exists
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty != null)
                    {
                        idProperty.SetValue(data, document.Id);
                    }

                    results.Add(data);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting collection {collection}");
                throw;
            }
        }

        public async Task<List<T>> QueryCollectionAsync<T>(string collection, string field, object value) where T : class
        {
            try
            {
                Query query = _firestoreDb.Collection(collection).WhereEqualTo(field, value);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();
                var results = new List<T>();

                foreach (var document in snapshot.Documents)
                {
                    var data = document.ConvertTo<T>();

                    // Set Id property if it exists
                    var idProperty = typeof(T).GetProperty("Id");
                    if (idProperty != null)
                    {
                        idProperty.SetValue(data, document.Id);
                    }

                    results.Add(data);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error querying collection {collection}");
                throw;
            }
        }
    }
}