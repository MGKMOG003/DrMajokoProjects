using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class Phase
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ProjectId { get; set; }

        [FirestoreProperty]
        public string PhaseName { get; set; }

        [FirestoreProperty]
        public double PhaseBudget { get; set; }

        [FirestoreProperty]
        public double PhaseCost { get; set; }

        [FirestoreProperty]
        public DateTime EstimatedStartDate { get; set; }

        [FirestoreProperty]
        public DateTime EstimatedEndDate { get; set; }

        [FirestoreProperty]
        public DateTime? ActualStartDate { get; set; }

        [FirestoreProperty]
        public DateTime? ActualEndDate { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } // Pending, In Progress, Done

        [FirestoreProperty]
        public int OrderIndex { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public DateTime UpdatedAt { get; set; }
    }
}