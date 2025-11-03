using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class Contractor
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string PhoneNumber { get; set; }

        [FirestoreProperty]
        public string Specialty { get; set; }

        [FirestoreProperty]
        public double Rating { get; set; }

        [FirestoreProperty]
        public int CompletedTasks { get; set; }

        [FirestoreProperty]
        public bool IsAvailable { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }
    }
}