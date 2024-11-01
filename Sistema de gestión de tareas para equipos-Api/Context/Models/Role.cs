using System;
using System.ComponentModel.DataAnnotations;
using Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models.Common;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models
{
    public class Role : BaseEntity
    {
        [Required]
        public string Name { get; set; } = null!;

        public ICollection<UserExtend> Users { get; set; } = null!;
    }
}
