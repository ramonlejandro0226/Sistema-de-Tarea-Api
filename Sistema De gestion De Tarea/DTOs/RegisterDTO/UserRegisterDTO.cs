using Microsoft.Identity.Client;

namespace Sistema_De_gestion_De_Tarea.DTOs.RegisterDTO
{
    public class UserRegisterDTO
    {

        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        
        public RoleDTO Role { get; set; } = null!;
    }
}
