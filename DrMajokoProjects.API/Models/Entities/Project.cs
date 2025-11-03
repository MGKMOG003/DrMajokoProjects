using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class Project
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public string ClientId { get; set; }

        [FirestoreProperty]
        public string ProjectManagerId { get; set; }

        [FirestoreProperty]
        public double Budget { get; set; }

        [FirestoreProperty]
        public double ActualSpent { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }

        [FirestoreProperty]
        public DateTime StartDate { get; set; }

        [FirestoreProperty]
        public DateTime? EndDate { get; set; }

        [FirestoreProperty]
        public string Location { get; set; }  // ADDED: Project location

        [FirestoreProperty]
        public List<ProjectPhase> Phases { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public DateTime UpdatedAt { get; set; }
    }

    [FirestoreData]
    public class ProjectPhase
    {
        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public DateTime StartDate { get; set; }

        [FirestoreProperty]
        public DateTime EndDate { get; set; }

        [FirestoreProperty]
        public double AllocatedBudget { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }
    }
}