
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace Sistema_De_gestion_De_Tarea.Context.Models
{
    public class User
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public Role Role { get; set; } = null!;

        [Required]
        public ICollection<AssignedToUser> AssignedToUser { get; set; } = null!;


    }
}
