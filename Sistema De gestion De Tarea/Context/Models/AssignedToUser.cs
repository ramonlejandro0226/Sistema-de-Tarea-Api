using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema_De_gestion_De_Tarea.Context.Models
{
    [Keyless]
    public class AssignedToUser
    {
        [ForeignKey("Task")]
         public  Guid TaskId  { get; set; }
   
        public Sistema_De_gestion_De_Tarea.Context.Models.Task Task { get; set; } = null!;
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        
        public User User { get; set; }= null!;

    }
}
