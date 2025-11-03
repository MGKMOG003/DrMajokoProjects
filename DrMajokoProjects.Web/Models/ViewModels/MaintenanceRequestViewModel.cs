namespace DrMajokoProjects.Web.Models.ViewModels
{
    public class MaintenanceRequestViewModel
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> PhotoUrls { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string AssignedContractorId { get; set; }  // Add this property
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CompletionNotes { get; set; }
    }

    public class CreateMaintenanceRequestViewModel
    {
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public List<IFormFile> Photos { get; set; }
    }
}