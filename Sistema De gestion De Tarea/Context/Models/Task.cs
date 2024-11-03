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

        public Status Status { get; set; }=null!;

        [Required]

        public ICollection<AssignedToUser> AssignedToUser { get; set; } = null!;

        public User User {  get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
