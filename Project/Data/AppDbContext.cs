using Microsoft.EntityFrameworkCore;
using Project.Models;
using Task = Project.Models.Task;
using Projectt = Project.Models.Projectt;


namespace Project.Data
{
    public class AppDbContext : DbContext //DbContext is a central class in Entity Framework Core that represents a session with the database and provides methods to query and save data.
    {
        public AppDbContext(DbContextOptions options) : base(options) { }
        //The DbContextOptions object typically contains configuration information,
        //including the connection string and other database-related settings.
        //The constructor passes these options to the base class (DbContext) constructor using base(options).

        public DbSet<User> Users { get; set; }

        public DbSet<Task> Tasks { get; set; }

        public DbSet<Projectt> Projects { get; set; }

        public DbSet<ProjectUser> ProjectUsers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.UserId, pu.ProjecttId });

            
            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany(u => u.ProjectUsers)
                .HasForeignKey(pu => pu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.Projectt)
                .WithMany(p => p.ProjectUsers)
                .HasForeignKey(pu => pu.ProjecttId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
