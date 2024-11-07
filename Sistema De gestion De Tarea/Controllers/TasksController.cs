using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema_De_gestion_De_Tarea.Context;
using Sistema_De_gestion_De_Tarea.Context.Models;
using Sistema_De_gestion_De_Tarea.DTOs.TaskDTO;
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
            var tasks = await _context.Tasks.Include(t => t.Status).Include(assigned => assigned.AssignedToUser).Where(t => t.CreatedByUserId == currentUserId).ToListAsync();
            if (tasks == null || tasks.Count == 0)
            {
                return NotFound("No se encontraron tareas para el usuario actual.");
            }

            return Ok(_mapper.Map<IEnumerable<TaskGetDTO>>(tasks));
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
        [HttpPut("UpdateTask/{id}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> UpdateTask(Guid id, TaskPutDTO taskUpdateDTO)
        {
            if (id != taskUpdateDTO.id)
            {
                return BadRequest("El ID de la tarea no coincide con el ID proporcionado en el cuerpo de la solicitud.");
            }

            // Obtener el ID del usuario actual
            var currentUserId = GetCurrentUserId();

            // Buscar la tarea específica creada por el usuario actual
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.CreatedByUserId == currentUserId && t.Id == id);

            if (task == null)
            {
                return NotFound("No se encontró la tarea para el usuario actual.");
            }

            // Verificar si el estado existe en la base de datos
            var status = await _context.Status
                .SingleOrDefaultAsync(s => s.Name.ToLower() == taskUpdateDTO.Status.Name.ToLower());

            if (status == null)
            {
                return BadRequest("El estado proporcionado no existe.");
            }

            // Actualizar solo las propiedades permitidas
            task.Description = taskUpdateDTO.Description;
            task.Status = status;

            // Guardar cambios en la base de datos
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest("Invalid Task ID.");
            }

            var currentUserId = GetCurrentUserId();
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.CreatedByUserId == currentUserId && t.Id == id);

            if (task == null)
            {
                return NotFound("Task not found or you don't have permission to delete it.");
            }

            //Permitir eliminación solo para el creador o un administrador
            if (!IsAdmin(currentUserId))
            {
                return Forbid();
            }

            _context.Tasks.Remove(task);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, "An error occurred while deleting the task.");
            }

            return NoContent();
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
