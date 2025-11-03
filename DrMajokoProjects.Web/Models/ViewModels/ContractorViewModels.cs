namespace DrMajokoProjects.Web.Models.ViewModels
{
    // Dashboard
    public class ContractorDashboardViewModel
    {
        public string ContractorId { get; set; }
        public string ContractorName { get; set; }
        public List<ProjectViewModel> AssignedProjects { get; set; }
        public List<ContractorTaskViewModel> AllTasks { get; set; }
        public PerformanceMetrics OverallPerformance { get; set; }
        public List<PhasePerformance> PhasePerformances { get; set; }
        public List<QuotationViewModel> RecentQuotations { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int DelayedTasks { get; set; }
        public int PendingQuotations { get; set; }
        public double AverageRating { get; set; }
    }

    // Task specific for contractor
    public class ContractorTaskViewModel
    {
        public string Id { get; set; }
        public string TaskName { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string PhaseId { get; set; }
        public string PhaseName { get; set; }
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public double EstimatedCost { get; set; }
        public double ActualCost { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public List<string> CompletionPhotoUrls { get; set; }
        public string CompletionNotes { get; set; }
        public int DaysRemaining => (EstimatedEndDate - DateTime.Now).Days;
        public int DaysLate => ActualEndDate.HasValue && ActualEndDate > EstimatedEndDate
            ? (ActualEndDate.Value - EstimatedEndDate).Days : 0;
        public bool IsOverdue => Status != "Done" && DateTime.Now > EstimatedEndDate;
        public bool HasQuotation { get; set; }
    }

    // Performance Metrics
    public class PerformanceMetrics
    {
        public double OnTrackPercentage { get; set; }
        public double DelayedPercentage { get; set; }
        public double InProgressPercentage { get; set; }
        public double AverageDelayDays { get; set; }
        public double CompletionRate { get; set; }
        public double QualityRating { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int TotalTasksOnTime { get; set; }
    }

    public class PhasePerformance
    {
        public string PhaseId { get; set; }
        public string PhaseName { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OnTimeTasks { get; set; }
        public int DelayedTasks { get; set; }
        public double AverageDelayDays { get; set; }
        public double CompletionPercentage { get; set; }
    }

    // Quotation for contractor
    public class ContractorQuotationViewModel
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ContractorId { get; set; }
        public double QuotationAmount { get; set; }
        public double MaterialCost { get; set; }
        public double LaborCost { get; set; }
        public string Description { get; set; }
        public List<string> AttachmentUrls { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string RejectionReason { get; set; }
    }

    public class CreateContractorQuotationViewModel
    {
        public string TaskId { get; set; }
        public string ProjectId { get; set; }
        public string ContractorId { get; set; }
        public string ContractorName { get; set; }
        public double MaterialCost { get; set; }
        public double LaborCost { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Attachments { get; set; }
    }

    public class UpdateTaskStatusViewModel
    {
        public string TaskId { get; set; }
        public string Status { get; set; }
        public double? ActualCost { get; set; }
        public string CompletionNotes { get; set; }
        public List<IFormFile> CompletionPhotos { get; set; }
    }
}