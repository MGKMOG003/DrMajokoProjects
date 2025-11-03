namespace DrMajokoProjects.Web.Models.ViewModels
{
    public class ClientDashboardViewModel
    {
        public string ClientId { get; set; }
        public List<ProjectViewModel> Projects { get; set; }
        public List<MaintenanceRequestViewModel> RecentMaintenanceRequests { get; set; }
        public int TotalProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequests { get; set; }
    }

    public class ClientProjectDetailsViewModel
    {
        public ProjectViewModel Project { get; set; }
        public List<MaintenanceRequestViewModel> MaintenanceRequests { get; set; }
        public List<DocumentViewModel> Documents { get; set; }
    }

    public class DocumentViewModel
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
    public class MessageViewModel
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string MessageText { get; set; }
        public string AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }

    public class SendMessageViewModel
    {
        public string ProjectId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string MessageText { get; set; }
        public IFormFile Attachment { get; set; }
    }

    public class ChatProjectViewModel
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int UnreadCount { get; set; }
        public DateTime LastMessageTime { get; set; }
    }
}