namespace Project.DTO
{
    public class ProjectCreationDto
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> UserIds { get; set; }
    }
}
