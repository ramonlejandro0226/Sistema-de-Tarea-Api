using Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models
{
    public class TaskHistory : BaseEntity
    {
        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public Task Task { get; set; } = null!;

        [Required]
        public string ChangeType { get; set; } = null!;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        public string ChangedByUserId { get; set; } = null!;

        [ForeignKey("ChangedByUserId")]
        public UserExtend ChangedByUser { get; set; } = null!;
    }
}
