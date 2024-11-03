using Microsoft.EntityFrameworkCore;
using Sistema_De_gestion_De_Tarea.Context.Models;
using Task = Sistema_De_gestion_De_Tarea.Context.Models.Task;

namespace Sistema_De_gestion_De_Tarea.Context
{
    public class ApplicationDbContext:DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base (options) {
         
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AssignedToUser> AssignedToUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AssignedToUser>().HasKey(x => new { x.UserId, x.TaskId });

            modelBuilder.Entity<Task>().HasMany(t => t.AssignedToUser).WithOne(x => x.Task).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(t => t.AssignedToUser).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
