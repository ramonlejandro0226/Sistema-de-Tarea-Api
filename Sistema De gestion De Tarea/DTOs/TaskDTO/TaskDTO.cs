namespace Sistema_De_gestion_De_Tarea.DTOs.TaskDTO
{
    public class TaskDTO
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public StatusDTO Status { get; set; } = null!;
        public ICollection<AssignedToUserDTO> AssignedToUser { get; set; } = new List<AssignedToUserDTO>();

        // Agregar UserName para verificar el usuario en la base de datos
        public string UserName { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Agregar CreatedByUserId si es necesario
        public Guid CreatedByUserId { get; set; }
    }
}

