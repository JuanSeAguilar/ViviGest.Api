using Microsoft.AspNetCore.Mvc;
using ViviGest.Api.Dtos;
using ViviGest.Api.Models;
using ViviGest.Api.Services;
using ViviGest.Data;

[ApiController]
[Route("api/[controller]")]
public class RegistroController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;

    public RegistroController(AppDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    [HttpPost("usuario")]
    public IActionResult RegistrarUsuario([FromBody] RegistrarUsuarioDto dto)
    {
        // Crear Persona
        var persona = new Persona
        {
            IdTipoDocumento = dto.IdTipoDocumento,
            NumeroDocumento = dto.NumeroDocumento,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            Telefono = dto.Telefono,
            CorreoElectronico = dto.Correo
        };
        _context.Personas.Add(persona);
        _context.SaveChanges();

        // Crear Usuario con hash/salt
        _passwordService.CreatePasswordHash(dto.Contrasena, out var hash, out var salt);

        var usuario = new Usuario
        {
            IdPersona = persona.IdPersona,
            ContrasenaHash = hash,
            ContrasenaSalt = salt
        };

        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        // Rol
        var usuarioRol = new UsuarioRol
        {
            IdUsuario = usuario.IdUsuario,
            IdRol = dto.IdRol
        };
        _context.UsuarioRoles.Add(usuarioRol);
        _context.SaveChanges();

        return Ok(new { message = "Usuario registrado correctamente" });
    }
}
