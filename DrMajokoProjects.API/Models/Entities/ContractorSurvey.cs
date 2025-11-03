using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class ContractorSurvey
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string TaskId { get; set; }

        [FirestoreProperty]
        public string ContractorId { get; set; }

        [FirestoreProperty]
        public string ProjectManagerId { get; set; }

        [FirestoreProperty]
        public int TimelinessRating { get; set; } // 1-5

        [FirestoreProperty]
        public int QualityRating { get; set; } // 1-5

        [FirestoreProperty]
        public int CommunicationRating { get; set; } // 1-5

        [FirestoreProperty]
        public int ProfessionalismRating { get; set; } // 1-5

        [FirestoreProperty]
        public double OverallRating { get; set; }

        [FirestoreProperty]
        public string Comments { get; set; }

        [FirestoreProperty]
        public bool WouldRecommend { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }
    }
}