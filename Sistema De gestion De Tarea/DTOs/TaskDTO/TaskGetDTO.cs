namespace Sistema_De_gestion_De_Tarea.DTOs.TaskDTO
{
    public class TaskGetDTO
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public StatusDTO Status { get; set; } = null!;
        public ICollection<AssignedToUserDTO> AssignedToUser { get; set; } = new List<AssignedToUserDTO>();

        public DateTime CreatedAt { get; set; }


        public Guid CreatedByUserId { get; set; } 
    }
}
