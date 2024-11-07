using AutoMapper;
using Sistema_De_gestion_De_Tarea.Context.Models;
using Sistema_De_gestion_De_Tarea.DTOs.LoginDTO;
using Sistema_De_gestion_De_Tarea.DTOs.RegisterDTO;
using Sistema_De_gestion_De_Tarea.DTOs.TaskDTO;
using Task = Sistema_De_gestion_De_Tarea.Context.Models.Task;

namespace Sistema_De_gestion_De_Tarea.Mapping
{
    public class ProfileMappings : Profile
    {
        public ProfileMappings()
        {
            CreateMap<Task, TaskDTO>()
         .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<UserDTO, User>().ReverseMap();

            CreateMap<Task, TaskGetDTO>().ReverseMap();

            CreateMap<UserRegisterDTO, User>().ReverseMap();
            CreateMap<RoleDTO, Role>().ReverseMap();

            CreateMap<StatusDTO, Status>().ReverseMap();

            CreateMap<AssignedToUserDTO, AssignedToUser>().ReverseMap();

            CreateMap<LoginDTO, User>().ReverseMap();

        }

    }

}
