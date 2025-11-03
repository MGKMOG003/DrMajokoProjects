namespace DrMajokoProjects.Web.Models.ViewModels
{
    public class CreateProjectViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientId { get; set; }
        public string ProjectManagerId { get; set; }  // Must have this
        public double Budget { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }  // Must have this
    }

    public class ProjectViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ClientId { get; set; }
        public string ProjectManagerId { get; set; }  // Must have this
        public double Budget { get; set; }
        public double ActualSpent { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Location { get; set; }  // Must have this
    }
}