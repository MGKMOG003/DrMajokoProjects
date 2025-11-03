using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string FirebaseUid { get; set; }  // Firebase Auth UID

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string PhoneNumber { get; set; }

        [FirestoreProperty]
        public string Role { get; set; }  // Admin, Project Manager, Contractor, Client

        [FirestoreProperty]
        public string Status { get; set; }  // Pending, Approved, Denied

        [FirestoreProperty]
        public string ProfilePictureUrl { get; set; }

        [FirestoreProperty]
        public double Rating { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public DateTime UpdatedAt { get; set; }

        [FirestoreProperty]
        public DateTime? LastLoginAt { get; set; }
    }
}