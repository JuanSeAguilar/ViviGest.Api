using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ViviGest.Api.Models;
using ViviGest.Api.Services;
using ViviGest.Data;

namespace ViviGest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{ 
    private readonly AppDbContext _db;
    private readonly IPasswordService _pwd;
    private readonly IConfiguration _cfg;

    public AuthController(AppDbContext db, IPasswordService pwd, IConfiguration cfg)
    {
        _db = db;
        _pwd = pwd;
        _cfg = cfg;
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Usuario y contraseña son requeridos" });

        // Busca por correo en Persona y trae Usuario + Roles
        var persona = await _db.Personas
            .Include(p => p.Usuario)
                .ThenInclude(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(p => p.CorreoElectronico != null && p.CorreoElectronico == req.Username);

        if (persona?.Usuario == null || !persona.Usuario.Activo)
            return Unauthorized(new { message = "Credenciales inválidas" });

        var u = persona.Usuario;

        if (!_pwd.VerifyPassword(req.Password, u.ContrasenaHash, u.ContrasenaSalt))
            return Unauthorized(new { message = "Credenciales inválidas" });

        var roles = u.UsuarioRoles.Select(r => r.Rol.Nombre).ToList();
        var token = GenerateJwt(persona, roles);

        return Ok(new LoginResponse
        {
            Token = token,
            Username = req.Username,
            Roles = roles
        });
    }

    private string GenerateJwt(Persona persona, IEnumerable<string> roles)
    {
        var u = persona.Usuario!; // ya validaste que existe

        var claims = new List<Claim>
    {
        // 👇 El controlador leerá cualquiera de estos como IdUsuario (GUID)
        new Claim(JwtRegisteredClaimNames.Sub, u.IdUsuario.ToString()),            // subject = IdUsuario
        new Claim(ClaimTypes.NameIdentifier, u.IdUsuario.ToString()),              // nameid
        new Claim("userId", u.IdUsuario.ToString()),                               // alias
        new Claim("uid", u.IdUsuario.ToString()),                                  // alias

        // Info adicional útil
        new Claim(ClaimTypes.Email, persona.CorreoElectronico ?? string.Empty),
        new Claim("pid", persona.IdPersona.ToString())                             // IdPersona (por si acaso)
    };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _cfg["Jwt:Issuer"],
            audience: _cfg["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}

