using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Services.Interfaces
{
    public interface IFirebaseService
    {
        FirestoreDb GetFirestoreDb();
        Task<T> GetDocumentAsync<T>(string collection, string documentId) where T : class;
        Task<string> AddDocumentAsync<T>(string collection, T data) where T : class;
        Task UpdateDocumentAsync<T>(string collection, string documentId, T data) where T : class;
        Task DeleteDocumentAsync(string collection, string documentId);
        Task<List<T>> GetCollectionAsync<T>(string collection) where T : class;
        Task<List<T>> QueryCollectionAsync<T>(string collection, string field, object value) where T : class;
    }
}