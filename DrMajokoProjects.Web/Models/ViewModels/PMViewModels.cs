namespace DrMajokoProjects.Web.Models.ViewModels
{
    // Dashboard
    public class PMDashboardViewModel
    {
        public string ProjectManagerId { get; set; }
        public List<ProjectViewModel> Projects { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int PendingQuotations { get; set; }
        public int OverdueTasks { get; set; }
    }

    // Phases
    public class PhaseViewModel
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string PhaseName { get; set; }
        public double PhaseBudget { get; set; }
        public double PhaseCost { get; set; }
        public double RemainingBudget => PhaseBudget - PhaseCost;
        public double PercentageUsed => PhaseBudget > 0 ? (PhaseCost / PhaseBudget) * 100 : 0;
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string Status { get; set; }
        public int OrderIndex { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
        public double CompletionPercentage { get; set; }
    }

    public class CreatePhaseViewModel
    {
        public string ProjectId { get; set; }
        public string PhaseName { get; set; }
        public double PhaseBudget { get; set; }
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public int OrderIndex { get; set; }
    }

    // Tasks
    public class TaskViewModel
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string PhaseId { get; set; }
        public string TaskName { get; set; }
        public string AssignedContractorId { get; set; }
        public string AssignedContractorName { get; set; }
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public double EstimatedCost { get; set; }
        public double ActualCost { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string DependsOnTaskId { get; set; }
        public List<string> CompletionPhotoUrls { get; set; }
        public string CompletionNotes { get; set; }
        public int DaysLate => ActualEndDate.HasValue && ActualEndDate > EstimatedEndDate
            ? (ActualEndDate.Value - EstimatedEndDate).Days : 0;
        public bool IsOverdue => Status != "Done" && DateTime.Now > EstimatedEndDate;
    }

    public class CreateTaskViewModel
    {
        public string ProjectId { get; set; }
        public string PhaseId { get; set; }
        public string TaskName { get; set; }
        public string AssignedContractorId { get; set; }
        public DateTime EstimatedStartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public double EstimatedCost { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string DependsOnTaskId { get; set; }
    }

    // Quotations
    public class QuotationViewModel
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public string ProjectId { get; set; }
        public string ContractorId { get; set; }
        public string ContractorName { get; set; }
        public string QuotationType { get; set; }  // "General" or "Task"

        // Cost breakdown
        public double MaterialCost { get; set; }
        public double LaborCost { get; set; }
        public double SubTotal { get; set; }
        public double Tax { get; set; }
        public double QuotationAmount { get; set; }

        // ADD THIS: Calculated total amount
        public double TotalAmount => QuotationAmount > 0 ? QuotationAmount : MaterialCost + LaborCost;

        // Details
        public string Description { get; set; }
        public List<string> AttachmentUrls { get; set; }

        // Status tracking
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedBy { get; set; }
        public string RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Timeline/Gantt
    public class TimelineViewModel
    {
        public ProjectViewModel Project { get; set; }
        public List<PhaseViewModel> Phases { get; set; }
        public double TotalBudget { get; set; }
        public double TotalCost { get; set; }
        public double RemainingBudget => TotalBudget - TotalCost;
        public double OverallProgress { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
    }

    // Budget Summary
    public class BudgetSummaryViewModel
    {
        public ProjectViewModel Project { get; set; }
        public double TotalProjectBudget { get; set; }
        public double TotalAllocatedBudget { get; set; }
        public double TotalSpent { get; set; }
        public double RemainingBudget { get; set; }
        public double PercentageSpent { get; set; }
        public List<PhaseViewModel> Phases { get; set; }
        public List<BudgetAlert> Alerts { get; set; }
    }

    public class BudgetAlert
    {
        public string Type { get; set; } // Warning, Danger
        public string Message { get; set; }
        public string PhaseId { get; set; }
        public string PhaseName { get; set; }
    }

    // Reports
    public class PhaseReportViewModel
    {
        public PhaseViewModel Phase { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int DelayedTasks { get; set; }
        public int OnTimeTasks { get; set; }
        public double AverageDelayDays { get; set; }
        public double TotalEstimatedDays { get; set; }
        public double TotalActualDays { get; set; }
        public DateTime? PhaseStartDate { get; set; }
        public DateTime? PhaseEndDate { get; set; }
    }

    // Surveys
    public class ContractorSurveyViewModel
    {
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        public string ContractorId { get; set; }
        public string ContractorName { get; set; }
        public string ProjectManagerId { get; set; }
        public int TimelinessRating { get; set; }
        public int QualityRating { get; set; }
        public int CommunicationRating { get; set; }
        public int ProfessionalismRating { get; set; }
        public double OverallRating { get; set; }  // ADD THIS
        public string Comments { get; set; }
        public bool WouldRecommend { get; set; }
        public DateTime CreatedAt { get; set; }  // ADD THIS
    }
}