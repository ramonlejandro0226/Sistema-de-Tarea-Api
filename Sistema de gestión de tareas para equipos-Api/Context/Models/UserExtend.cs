using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models
{
    public class UserExtend : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [Required]
        public Guid RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; } = null!;

        public ICollection<TaskUser> TaskUsers { get; set; } = new List<TaskUser>();
        public ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
        public ICollection<TaskHistory> TaskHistories { get; set; } = new List<TaskHistory>();
    }
}
