using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_De_gestion_De_Tarea.Context.Models
{
    public class AssignedToUser
    {
        [Key]
        [Column(Order = 1)]
        public Guid UserId { get; set; }

        [Key]
        [Column(Order = 2)]
        public Guid TaskId { get; set; }

        public User User { get; set; } = null!;
        public Task Task { get; set; } = null!;
    }
}
