using Microsoft.EntityFrameworkCore;
using Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context
{
    public class TaskManagementDbContext : DbContext
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserExtend> Users { get; set; }
        public DbSet<Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models.Task> Tasks { get; set; }
        public DbSet<TaskUser> TaskUsers { get; set; }
        public DbSet<TaskHistory> TaskHistories { get; set; }

        public TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskUser>()
                .HasKey(tu => new { tu.TaskId, tu.UserId });

            modelBuilder.Entity<TaskUser>()
                .HasOne(tu => tu.Task)
                .WithMany(t => t.TaskUsers)
                .HasForeignKey(tu => tu.TaskId)
                .OnDelete(DeleteBehavior.NoAction);  // Especificar NoAction

            modelBuilder.Entity<TaskUser>()
                .HasOne(tu => tu.User)
                .WithMany(u => u.TaskUsers)
                .HasForeignKey(tu => tu.UserId)
                .OnDelete(DeleteBehavior.NoAction);  // Especificar NoAction

            modelBuilder.Entity<TaskHistory>()
                .HasOne(th => th.Task)
                .WithMany(t => t.TaskHistories)
                .HasForeignKey(th => th.TaskId)
                .OnDelete(DeleteBehavior.NoAction);  // Especificar NoAction

            modelBuilder.Entity<TaskHistory>()
                .HasOne(th => th.ChangedByUser)
                .WithMany(u => u.TaskHistories)
                .HasForeignKey(th => th.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);  // Especificar NoAction
        }
    }
}
