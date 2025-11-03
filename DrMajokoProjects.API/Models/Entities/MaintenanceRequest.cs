using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class MaintenanceRequest
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ProjectId { get; set; }

        [FirestoreProperty]
        public string ClientId { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public List<string> PhotoUrls { get; set; }

        [FirestoreProperty]
        public string Priority { get; set; } // Low, Medium, High, Urgent

        [FirestoreProperty]
        public string Status { get; set; } // Pending, Assigned, In Progress, Completed

        [FirestoreProperty]
        public string AssignedContractorId { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public DateTime? AssignedAt { get; set; }

        [FirestoreProperty]
        public DateTime? CompletedAt { get; set; }

        [FirestoreProperty]
        public string CompletionNotes { get; set; }
    }
}