using Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models
{
    public class Task : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public Status TaskStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<TaskUser> TaskUsers { get; set; } = null!;

        [Required]
        public int CreatedByUserId { get; set; }

        [ForeignKey("CreatedByUserId")]
        public UserExtend CreatedByUser { get; set; } = null!;

        public ICollection<TaskHistory> TaskHistories { get; set; } = null!;

        public enum Status
        {
            Pending,
            InProgress,
            Done
        }
    }
}
