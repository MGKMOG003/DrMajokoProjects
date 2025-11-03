using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class Document
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ProjectId { get; set; }

        [FirestoreProperty]
        public string FileName { get; set; }

        [FirestoreProperty]
        public string FileType { get; set; } // Blueprint, Contract, Permit, Photo, Report

        [FirestoreProperty]
        public string FilePath { get; set; }

        [FirestoreProperty]
        public string FileUrl { get; set; }

        [FirestoreProperty]
        public long FileSize { get; set; }

        [FirestoreProperty]
        public string UploadedBy { get; set; }

        [FirestoreProperty]
        public DateTime UploadedAt { get; set; }
    }
}