namespace DrMajokoProjects.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // REMOVED: MessageViewModel - use the one from ClientViewModels.cs
    // REMOVED: SendMessageViewModel - use the one from ClientViewModels.cs

    public class AuditLogViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public string EntityId { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // REMOVED: ChatProjectViewModel - use the one from ClientViewModels.cs

    public class ReportViewModel
    {
        public List<ProjectBudgetSummary> ProjectSummaries { get; set; }
        public List<ContractorPerformance> ContractorPerformances { get; set; }
    }

    public class ProjectBudgetSummary
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public double TotalBudget { get; set; }
        public double AmountSpent { get; set; }
        public double RemainingBudget { get; set; }
        public double PercentageSpent { get; set; }
        public string Status { get; set; }
    }

    public class ContractorPerformance
    {
        public string ContractorId { get; set; }
        public string ContractorName { get; set; }
        public int TotalTasksAssigned { get; set; }
        public int TasksCompleted { get; set; }
        public int TasksOnTime { get; set; }
        public int TasksDelayed { get; set; }
        public double AverageRating { get; set; }
        public double CompletionRate { get; set; }
    }
}