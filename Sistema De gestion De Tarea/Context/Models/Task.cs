using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace Sistema_De_gestion_De_Tarea.Context.Models
{
    public class Task
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public Status Status { get; set; } = null!;

        // Relación con los usuarios asignados
        [Required]
        public ICollection<AssignedToUser> AssignedToUser { get; set; } = new List<AssignedToUser>();

        // ID del usuario que creó la tarea
        [Required]
        public Guid CreatedByUserId { get; set; } // Nueva propiedad para almacenar el ID del creador

        // Propiedad de navegación para el usuario creador

        // Fecha de creación de la tarea
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }


}
