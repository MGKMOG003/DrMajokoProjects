namespace DrMajokoProjects.API.Models.DTOs
{
    public class CreateMaintenanceRequestDto
    {
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
    }
}