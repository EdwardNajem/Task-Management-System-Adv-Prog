using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? Bio { get; set; }
        public bool IsAdmin { get; set; }
        public List<ProjectUser>? ProjectUsers { get; set; }
        public List<Task>? Tasks { get; set; }

    }
}
