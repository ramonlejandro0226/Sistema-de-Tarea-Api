namespace Sistema_De_gestion_De_Tarea.DTOs.TaskDTO
{
    public class TaskGetAssigned
    {
        public Guid id { get; set; } 
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public StatusDTO Status { get; set; } = null!;
        public string CreatedByUserName { get; set; } = null!; // Nombre del usuario que creó la tarea
        public DateTime CreatedAt { get; set; }
    }
}

