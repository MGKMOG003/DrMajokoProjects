using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class AuditLog
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string UserName { get; set; }

        [FirestoreProperty]
        public string Action { get; set; } // Login, Create Project, Upload File, Send Message, etc.

        [FirestoreProperty]
        public string EntityType { get; set; } // Project, Document, Message, User

        [FirestoreProperty]
        public string EntityId { get; set; }

        [FirestoreProperty]
        public string Details { get; set; }

        [FirestoreProperty]
        public DateTime Timestamp { get; set; }
    }
}