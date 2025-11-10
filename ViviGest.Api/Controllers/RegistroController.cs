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
    private readonly IEmailService _emailService;

    public RegistroController(AppDbContext context, IPasswordService passwordService, IEmailService emailService)
    {
        _context = context;
        _passwordService = passwordService;
        _emailService = emailService;
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

    [HttpPost("residente")]
    public async Task<IActionResult> RegistrarResidente([FromBody] RegistrarResidenteDto dto)
    {
        var unidad = _context.Unidades.FirstOrDefault(u => u.IdUnidad == dto.IdUnidad);
        if (unidad == null)
            return BadRequest("Unidad no encontrada");

        // 1️⃣ Crear Persona
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

        // 2️⃣ Crear Usuario con contraseña por defecto
        var passwordGenerica = dto.Contrasena ?? "123456";
        _passwordService.CreatePasswordHash(passwordGenerica, out var hash, out var salt);

        var usuario = new Usuario
        {
            IdPersona = persona.IdPersona,
            ContrasenaHash = hash,
            ContrasenaSalt = salt
        };
        _context.Usuarios.Add(usuario);
        _context.SaveChanges();

        // 3️⃣ Rol de residente
        var rolResidente = _context.Roles.FirstOrDefault(r => r.Nombre == "Residente");
        if (rolResidente == null)
            return BadRequest("El rol 'Residente' no existe.");

        _context.UsuarioRoles.Add(new UsuarioRol
        {
            IdUsuario = usuario.IdUsuario,
            IdRol = rolResidente.IdRol
        });
        _context.SaveChanges();

        // 4️⃣ Registrar Residencia
        _context.Residencias.Add(new Residencia
        {
            IdUsuario = usuario.IdUsuario,
            IdUnidad = dto.IdUnidad,
            FechaInicio = DateTime.Now
        });
        _context.SaveChanges();

        // 5️⃣ Enviar correo de bienvenida
        string cuerpo = $@"
            <h2>Bienvenido a ViviGest</h2>
            <p>Hola {persona.Nombres}, tu cuenta ha sido creada correctamente.</p>
            <p><b>Usuario:</b> {persona.CorreoElectronico}<br>
            <b>Contraseña:</b> {passwordGenerica}</p>
            <p>Unidad asignada: {unidad.Codigo}</p>
            <br/>
            <p>Gracias por usar ViviGest 🏠</p>";

        _emailService.EnviarCorreo(persona.CorreoElectronico!, "Bienvenido a ViviGest", cuerpo);

        return Ok(new
        {
            message = "✅ Residente registrado correctamente y correo enviado",
            residente = new { persona.Nombres, persona.Apellidos, unidad.Codigo }
        });
    }
    }
