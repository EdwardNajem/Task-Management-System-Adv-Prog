namespace Project.Models
{
    public class ProjectUser
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int ProjecttId { get; set; }
        public Projectt Projectt { get; set; }
    }

}
