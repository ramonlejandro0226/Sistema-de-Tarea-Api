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
using Task = Sistema_De_gestion_De_Tarea.Context.Models.Task;

namespace Sistema_De_gestion_De_Tarea.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TasksController(ApplicationDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Tasks?status={status}&userAssigned={userId}
        [HttpGet]
        //[Authorize(Roles = "admin,user")]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> GetTasks(string? status = null, Guid? userAssigned = null)
        {
            var query = _context.Tasks.AsQueryable();
            var currentUserId = GetCurrentUserId();

            if (User.IsInRole("admin"))
            {
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status.Name == status);
                }

                if (userAssigned.HasValue)
                {
                    query = query
                        .Include(t => t.AssignedToUser)
                        .ThenInclude(atu => atu.User)
                        .Where(t => t.AssignedToUser.Any(atu => atu.UserId == userAssigned.Value));
                }
            }
            else if (User.IsInRole("user"))
            {
                query = query.Where(t => t.User.Id == currentUserId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(t => t.Status.Name == status);
                }
            }

            var tasks = await query.Include(t => t.Status).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<TaskDTO>>(tasks)); // Mapea a DTO antes de devolver
        }

        // GET: api/Tasks
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
        //{
        //    // Carga las tareas junto con el estado y los usuarios asignados
        //    return await _context.Tasks
        //        .Include(t => t.Status) // Incluir el estado de la tarea
        //        .Include(t => t.AssignedToUser) // Incluir los usuarios asignados
        //        .ThenInclude(atu => atu.User) // Incluir el usuario dentro de AssignedToUser
        //        .ToListAsync();
        //}


        // PUT: api/Tasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> PutTask(Guid id, Task task)
        {
            // Verifica que el ID de la tarea coincida
            if (id != task.Id)
            {
                return BadRequest();
            }

            // Busca la tarea existente en la base de datos
            var existingTask = await _context.Tasks
                .Include(t => t.User) // Asegúrate de incluir el usuario que posee la tarea
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTask == null)
            {
                return NotFound();
            }

            // Verificar si el usuario que está haciendo la petición es el propietario de la tarea o un administrador
            var currentUserId = GetCurrentUserId(); // Método que obtendría el ID del usuario actual

            if (existingTask.User.Id != currentUserId && !IsAdmin(currentUserId)) // Verifica si no es el propietario y no es administrador
            {
                return Forbid(); // Prohibir la modificación si no es el propietario ni un administrador
            }

            // Solo se permiten modificaciones en el estado y la descripción
            existingTask.Description = task.Description;
            existingTask.Status = task.Status;

            _context.Entry(existingTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Método para obtener el ID del usuario actual
        private Guid GetCurrentUserId( )
        {
            // Verifica si el usuario está autenticado
            if (User.Identity.IsAuthenticated)
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




        [HttpPost]
        [Authorize(Roles = "admin,user")]
        public async Task<ActionResult<Task>> PostTask(Task task)
        {
            // Verificar si el usuario asignado existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == task.User.Id);
            if (!userExists)
            {
                return BadRequest("El usuario asignado no existe.");
            }

            // Aquí puedes añadir la lógica para comprobar si hay usuarios asignados si es necesario
            // Verifica que los usuarios asignados en AssignedToUser existen
            if (task.AssignedToUser != null)
            {
                foreach (var assignedUser in task.AssignedToUser)
                {
                    var assignedUserExists = await _context.Users.AnyAsync(u => u.Id == assignedUser.UserId);
                    if (!assignedUserExists)
                    {
                        return BadRequest($"El usuario asignado con ID {assignedUser.UserId} no existe.");
                    }
                }
            }

            // Agregar la tarea al contexto
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask", new { id = task.Id }, task);
        }





        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var task = await _context.Tasks
                .Include(t => t.User) // Incluye el usuario para verificar la propiedad User.Id
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId(); // Obtener el ID del usuario actual

            // Verificar si el usuario es el dueño de la tarea o un administrador
            if (task.User.Id != currentUserId && !IsAdmin(currentUserId))
            {
                return Forbid(); // Prohibir la eliminación si no es el propietario ni un administrador
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(Guid id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
