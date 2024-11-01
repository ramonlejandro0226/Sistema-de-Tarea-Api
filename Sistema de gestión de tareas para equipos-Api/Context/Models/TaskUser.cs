using Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models
{
    public class TaskUser : BaseEntity
    {
        [Required]
        public Guid TaskId { get; set; }

        [ForeignKey("TaskId")]
        public Task Task { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public UserExtend User { get; set; } = null!;
    }
}
