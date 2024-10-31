using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models.Common;

namespace Sistema_de_gestión_de_tareas_para_equipos_Api.Context.Models
{
    public class Role: BaseEntity
    {
   
        [Required]
        public string Name { get; set; } = null!;

        public ICollection<UserExtend> UserExtend { get; set; } = null!;
    }
}
