using System.ComponentModel.DataAnnotations;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models.Common
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
