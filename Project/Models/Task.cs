using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int AssignedById { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        public Projectt Project { get; set; }

        // Foreign key for User
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
    }

}
