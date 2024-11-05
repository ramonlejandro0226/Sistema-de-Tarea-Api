using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sistema_De_gestion_De_Tarea.Context;
using Sistema_De_gestion_De_Tarea.Context.Models;
using Sistema_De_gestion_De_Tarea.DTOs;
using Sistema_De_gestion_De_Tarea.DTOs.LoginDTO;
using Sistema_De_gestion_De_Tarea.DTOs.RegisterDTO;

namespace Sistema_De_gestion_De_Tarea.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UsersController(ApplicationDbContext context, IConfiguration configuration , IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }



        // POST: api/Auth/Register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] UserRegisterDTO userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User data is required.");
            }

            // Verifica si el email ya está registrado
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                return BadRequest("Email is already in use.");
            }

            // Crear el usuario y hash de la contraseña
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = userDto.Name,
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash), // Hash de la contraseña
                Role = new Role { Name = userDto.Role.Name } 
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }



        //public async Task<ActionResult> Register([FromBody] Register userDto)
        //{
        //    if (userDto == null)
        //    {
        //        return BadRequest("User data is required.");
        //    }

        //    if (userDto.Role?.Name != "admin" && userDto.Role?.Name != "user")
        //    {
        //        return BadRequest("Invalid role. Only 'admin' and 'user' roles are allowed.");
        //    }

        //    // Verifica si el email ya está registrado
        //    if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
        //    {
        //        return BadRequest("Email is already in use.");
        //    }

        //    // Crear el usuario y hash de la contraseña
        //    var user = new User
        //    {
        //        Id = Guid.NewGuid(),
        //        Name = userDto.Name,
        //        Email = userDto.Email,
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash), // Usa BCrypt para hashear
        //        Role = userDto.Role
        //    };

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return Ok("User registered successfully");
        //}


        // POST: api/Auth/Login
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDTO loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Role) // Incluye el rol aquí
                .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            // Verifica si el usuario existe
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.PasswordHash, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            // Generar el token JWT
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // Método para generar el token JWT
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "user") // Usa operador ?? para manejar valores nulos
            };

            var jwtKey = _configuration["JwtSettings:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
