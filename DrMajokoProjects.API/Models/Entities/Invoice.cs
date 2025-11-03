using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class Invoice
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string QuotationId { get; set; }

        [FirestoreProperty]
        public string ProjectId { get; set; }

        [FirestoreProperty]
        public string InvoiceNumber { get; set; }

        [FirestoreProperty]
        public double Amount { get; set; }  // Changed from decimal to double

        [FirestoreProperty]
        public string Status { get; set; }

        [FirestoreProperty]
        public DateTime IssueDate { get; set; }

        [FirestoreProperty]
        public DateTime DueDate { get; set; }

        [FirestoreProperty]
        public DateTime? PaidDate { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }
    }
}