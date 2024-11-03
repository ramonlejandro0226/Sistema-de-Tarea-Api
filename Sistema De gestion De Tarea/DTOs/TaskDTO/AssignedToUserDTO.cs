namespace Sistema_De_gestion_De_Tarea.DTOs.TaskDTO
{
    public class AssignedToUserDTO
    {
        public ICollection<UserDTO> User { get; set; } = null!;
    }
}
