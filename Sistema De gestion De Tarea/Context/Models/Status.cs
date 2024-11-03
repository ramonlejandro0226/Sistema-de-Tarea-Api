using System.ComponentModel.DataAnnotations;

namespace Sistema_De_gestion_De_Tarea.Context.Models
{
    public class Status
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;
    }
}
