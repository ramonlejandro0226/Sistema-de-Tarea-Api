using AutoMapper;
using Sistema_De_gestion_De_Tarea.DTOs.TaskDTO;

namespace Sistema_De_gestion_De_Tarea.Mapping
{
    public class ProfileMappings: Profile
    {
        public ProfileMappings() {
            CreateMap<TaskDTO, Task>().ReverseMap();
        }
     
    }
}
