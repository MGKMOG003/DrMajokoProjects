using Google.Cloud.Firestore;

namespace DrMajokoProjects.API.Models.Entities
{
    [FirestoreData]
    public class Quotation
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string ProjectId { get; set; }

        [FirestoreProperty]
        public string ClientId { get; set; }

        [FirestoreProperty]
        public string QuotationNumber { get; set; }

        // PM Dashboard: Task-specific quotation fields
        [FirestoreProperty]
        public string TaskId { get; set; }

        [FirestoreProperty]
        public string PhaseId { get; set; }

        [FirestoreProperty]
        public string ContractorId { get; set; }

        [FirestoreProperty]
        public string ContractorName { get; set; }

        [FirestoreProperty]
        public string QuotationType { get; set; } // "General" or "Task"

        [FirestoreProperty]
        public double MaterialCost { get; set; }

        [FirestoreProperty]
        public double LaborCost { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public List<string> AttachmentUrls { get; set; }

        // Original fields
        [FirestoreProperty]
        public List<QuotationItem> Items { get; set; }

        [FirestoreProperty]
        public double SubTotal { get; set; }

        [FirestoreProperty]
        public double Tax { get; set; }

        [FirestoreProperty]
        public double TotalAmount { get; set; }

        [FirestoreProperty]
        public string Status { get; set; } // Draft, Submitted, Approved, Rejected, Accepted

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public DateTime? AcceptedAt { get; set; }

        // PM Dashboard: Review fields
        [FirestoreProperty]
        public DateTime? SubmittedAt { get; set; }

        [FirestoreProperty]
        public DateTime? ReviewedAt { get; set; }

        [FirestoreProperty]
        public string ReviewedBy { get; set; }

        [FirestoreProperty]
        public string RejectionReason { get; set; }

        [FirestoreProperty]
        public DateTime UpdatedAt { get; set; }
    }

    [FirestoreData]
    public class QuotationItem
    {
        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public int Quantity { get; set; }

        [FirestoreProperty]
        public double UnitPrice { get; set; }

        [FirestoreProperty]
        public double Total { get; set; }
    }
}