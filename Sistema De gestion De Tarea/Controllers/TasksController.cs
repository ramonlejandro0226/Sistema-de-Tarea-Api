using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_De_gestion_De_Tarea.Context;
using Sistema_De_gestion_De_Tarea.Context.Models;
using Sistema_De_gestion_De_Tarea.DTOs.TaskDTO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Task = Sistema_De_gestion_De_Tarea.Context.Models.Task;

namespace Sistema_De_gestion_De_Tarea.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TasksController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Tasks?status={status}&userAssigned={userId}

        [HttpGet("admin/tasks")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<TaskGetAssigned>>> GetAllTasksForAdmin()
        {
            // Obtener todas las tareas
            var tasks = await _context.Tasks
                .Include(t => t.Status) // Incluir el estado de la tarea
                .Include(t => t.AssignedToUser) // Incluir el usuario que creó la tarea
                .Select(t => new TaskGetAssigned
                {
                    Name = t.Name,
                    Description = t.Description,
                    Status = new StatusDTO { Name = t.Status.Name }, // Obtener el nombre del estado
                    CreatedByUserName = _context.Users
                        .Where(u => u.Id == t.CreatedByUserId) // Buscar el nombre del usuario creador
                        .Select(u => u.Name)
                        .FirstOrDefault() ?? "Unknown", // Si no se encuentra, devolver "Unknown"
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            if (tasks == null || tasks.Count == 0)
            {
                return NotFound("No se encontraron tareas.");
            }

            return Ok(tasks);
        }


        [HttpGet("my-tasks")]
        [Authorize(Roles = "admin,user")]
        public async Task<ActionResult<IEnumerable<TaskGetDTO>>> GetTask()
        {
            // Obtener el ID del usuario actual
            var currentUserId = GetCurrentUserId();

            // Buscar las tareas creadas por el usuario actual
            var tasks = await _context.Tasks
                .Include(t => t.Status) // Asegúrate de incluir el estado
                                .Where(t => t.CreatedByUserId == currentUserId) // Filtrar por el creador
                .Select(t => new TaskGetDTO
                {
                    Name = t.Name,
                    Description = t.Description,
                    Status = new StatusDTO { Name = t.Status.Name }, // Convertir a StatusDTO
                    CreatedByUserId = t.CreatedByUserId,
                    AssignedToUser = t.AssignedToUser.Select(au => new AssignedToUserDTO
                    {
                        UserId = au.UserId,
                        TaskId = au.TaskId
                    }).ToList(),
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            if (tasks == null || tasks.Count == 0)
            {
                return NotFound("No se encontraron tareas para el usuario actual.");
            }

            return Ok(tasks);
        }


        [HttpGet("my-Assigned tarea")]
        [Authorize(Roles = "admin,user")]
        public async Task<ActionResult<IEnumerable<TaskGetAssigned>>> GetAssignedTasks()
        {
            // Obtener el ID del usuario actual
            var currentUserId = GetCurrentUserId();

            // Buscar las tareas asignadas al usuario actual
            var tasks = await _context.Tasks
                .Where(t => t.AssignedToUser.Any(au => au.UserId == currentUserId)) // Filtrar por tareas asignadas al usuario actual
                .Include(t => t.Status) // Asegurarnos de incluir el estado
                .Select(t => new TaskGetAssigned
                {
                    Name = t.Name,
                    Description = t.Description,
                    Status = new StatusDTO { Name = t.Status.Name },
                    CreatedByUserName = _context.Users
                        .Where(u => u.Id == t.CreatedByUserId)
                        .Select(u => u.Name)
                        .FirstOrDefault(), // Obtener el nombre del usuario creador
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            if (tasks == null || tasks.Count == 0)
            {
                return NotFound("No se encontraron tareas asignadas para el usuario actual.");
            }

            return Ok(tasks);
        }




        // Método para verificar si el usuario es administrador
        private bool IsAdmin(Guid userId)
        {
            // Verifica si el usuario está autenticado
            if (User.Identity.IsAuthenticated)
            {
                // Obtén el claim que contiene el rol del usuario
                var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

                // Verifica si el rol es "Admin" (o el nombre que hayas definido para el rol de administrador)
                return roleClaim != null && roleClaim.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            }

            return false; // No está autenticado, así que no es administrador
        }

        // POST: api/Tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754


        [HttpPost("Created-Task")]
        [Authorize(Roles = "admin,user")]
        public async Task<ActionResult<Task>> PostTask(TaskDTO taskDTO)
        {
            // Obtener el ID del usuario actual (creador de la tarea)
            var currentUserId = GetCurrentUserId();

            // Verificar si el estado existe en la base de datos
            var status = await _context.Status
                .SingleOrDefaultAsync(s => s.Name.ToLower() == taskDTO.Status.Name.ToLower());
            if (status == null)
            {
                return BadRequest("El estado no existe.");
            }

            // Crear la tarea con el ID del creador
            var task = new Task
            {
                Id = Guid.NewGuid(),
                Name = taskDTO.Name,
                Description = taskDTO.Description,
                Status = status, // Asignar el estado recuperado
                CreatedAt = DateTime.UtcNow, // Mejor utilizar UTC
                CreatedByUserId = currentUserId, // Almacenar el creador de la tarea
                AssignedToUser = new List<AssignedToUser>() // Inicializar la lista vacía
            };

            // Crear las relaciones de usuarios asignados en `AssignedToUser`
            var assignedUsers = new List<AssignedToUser>();
            foreach (var assignedToUserDTO in taskDTO.AssignedToUser)
            {
                // Buscar el usuario por nombre usando taskDTO.UserName
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Name.ToLower() == taskDTO.UserName.ToLower());
                if (user == null)
                {
                    return BadRequest($"El usuario asignado con nombre '{taskDTO.UserName}' no existe.");
                }

                assignedUsers.Add(new AssignedToUser
                {
                    UserId = user.Id,
                    TaskId = task.Id // Asignar el TaskId recién creado
                });
            }

            // Agregar la tarea al contexto y guardar
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Agregar las relaciones de usuarios asignados y guardar
            _context.AssignedToUsers.AddRange(assignedUsers);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask", new { id = task.Id }, task);
        }




        // PUT: api/Tasks/UpdateTask/{id} ------------------------OJO ESTA EN PRUEBA
        [HttpPut("UpdateTask")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> UpdateTask(TaskPutDTO taskUpdateDTO)
        {


            // Obtener el ID del usuario actual
            var currentUserId = GetCurrentUserId();

            // Buscar las tareas creadas por el usuario actual
            var tasks = await _context.Tasks
                .Where(t => t.CreatedByUserId == currentUserId) // Filtrar por el creador
                .Include(t => t.Status) // Asegúrate de incluir el estado
                .ToListAsync();

            if (tasks == null || tasks.Count == 0)
            {
                return NotFound("No se encontraron tareas para el usuario actual.");
            }

            // Modificar solo la descripción y el estado para cada tarea
            foreach (var task in tasks)
            {
                // Actualizar la descripción
                task.Description = taskUpdateDTO.Description;

                // Verificar si el estado existe en la base de datos
                var status = await _context.Status
                    .SingleOrDefaultAsync(s => s.Name.ToLower() == taskUpdateDTO.Status.Name.ToLower());

                if (status == null)
                {
                    return BadRequest("El estado no existe.");
                }

                // Asignar el nuevo estado
                task.Status = status;
            }

            // Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            return NoContent();



        }




        Guid GetUserIdByName(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.Name == userName);
            if (user == null)
            {
                throw new Exception($"El usuario con el nombre '{userName}' no existe.");
            }
            return user.Id;
        }









        // DELETE: api/Tasks/5
        //    [HttpDelete("{id}")]
        //    [Authorize(Roles = "admin,user")]
        //    public async Task<IActionResult> DeleteTask(Guid id)
        //    {
        //        var task = await _context.Tasks
        //            .Include(t => t.User) // Incluye el usuario para verificar la propiedad User.Id
        //            .FirstOrDefaultAsync(t => t.Id == id);

        //        if (task == null)
        //        {
        //            return NotFound();
        //        }

        //        var currentUserId = GetCurrentUserId(); // Obtener el ID del usuario actual

        //        // Verificar si el usuario es el dueño de la tarea o un administrador
        //        if (task.User.Id != currentUserId && !IsAdmin(currentUserId))
        //        {
        //            return Forbid(); // Prohibir la eliminación si no es el propietario ni un administrador
        //        }

        //        _context.Tasks.Remove(task);
        //        await _context.SaveChangesAsync();

        //        return NoContent();
        //    }

        //    private bool TaskExists(Guid id)
        //    {
        //        return _context.Tasks.Any(e => e.Id == id);
        //    }
        //}



        // Método para obtener el ID del usuario actual
        private Guid GetCurrentUserId()
        {
            // Verifica si el usuario está autenticado
            if (User.Identity?.IsAuthenticated == true)
            {
                // Obtiene el claim que contiene el ID del usuario
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // o el tipo de claim que estés usando para el ID

                // Intenta convertir el valor del claim a Guid
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid userId))
                {
                    return userId; // Retorna el ID del usuario
                }
            }

            throw new UnauthorizedAccessException("User is not authenticated."); // Opcional: lanzar excepción si no está autenticado
        }

    }
}
