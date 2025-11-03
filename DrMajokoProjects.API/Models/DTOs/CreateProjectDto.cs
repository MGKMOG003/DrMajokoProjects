namespace DrMajokoProjects.API.Models.DTOs
{
    public class CreateProjectDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientId { get; set; }
        public string ProjectManagerId { get; set; }
        public double Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }  // ADDED: Project location
    }
}