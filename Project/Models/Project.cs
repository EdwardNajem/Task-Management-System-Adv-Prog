namespace Project.Models
{
    public class Projectt
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        List<Task>? Tasks { get; set; }
        public List<ProjectUser>? ProjectUsers { get; set; }

    }
}
