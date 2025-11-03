using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class Message
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ProjectId { get; set; }

        [FirestoreProperty]
        public string SenderId { get; set; }

        [FirestoreProperty]
        public string SenderName { get; set; }

        [FirestoreProperty]
        public string MessageText { get; set; }

        [FirestoreProperty]
        public string AttachmentUrl { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public bool IsRead { get; set; }
    }
}