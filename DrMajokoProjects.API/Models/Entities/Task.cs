using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class ProjectTask
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ProjectId { get; set; }

        [FirestoreProperty]
        public string PhaseId { get; set; }

        [FirestoreProperty]
        public string TaskName { get; set; }

        [FirestoreProperty]
        public string AssignedContractorId { get; set; }

        [FirestoreProperty]
        public DateTime EstimatedStartDate { get; set; }

        [FirestoreProperty]
        public DateTime EstimatedEndDate { get; set; }

        [FirestoreProperty]
        public DateTime? ActualStartDate { get; set; }

        [FirestoreProperty]
        public DateTime? ActualEndDate { get; set; }

        [FirestoreProperty]
        public double EstimatedCost { get; set; }

        [FirestoreProperty]
        public double ActualCost { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } // Scheduled, In Progress, Delayed, Done

        [FirestoreProperty]
        public string Priority { get; set; } // Low, Medium, High, Urgent

        [FirestoreProperty]
        public string DependsOnTaskId { get; set; }

        [FirestoreProperty]
        public List<string> CompletionPhotoUrls { get; set; }

        [FirestoreProperty]
        public string CompletionNotes { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public DateTime UpdatedAt { get; set; }
    }
}