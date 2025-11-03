namespace DrMajokoProjects.Web.Models.ViewModels
{
    public class ContractorViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Specialty { get; set; }
        public double Rating { get; set; }
        public int CompletedTasks { get; set; }
        public bool IsAvailable { get; set; }
    }
}
