using System;

namespace Sistema_De_gestion_De_Tarea.DTOs.RegisterDTO
{
    public class UserResponseDTO
    {
        public Guid Id { get; set; }    
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
