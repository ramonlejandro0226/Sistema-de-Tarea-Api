namespace Sistema_De_gestion_De_Tarea.DTOs.TaskDTO
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public StatusDTO Status { get; set; }
        public ICollection<AssignedToUserDTO> AssignedToUser { get; set; } = null!;
        public UserDTO User { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

