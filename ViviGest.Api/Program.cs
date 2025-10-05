using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ViviGest.Api.Models;
using ViviGest.Api.Services;
using ViviGest.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("FrontPolicy", p => p
        .WithOrigins("http://localhost:5173", "http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ViviGest API", Version = "v1" });

    // ?? Config JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT así: Bearer {tu token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors("FrontPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ViviGest API v1");
    c.RoutePrefix = "swagger"; // hace que quede en /swagger
});

app.MapControllers();

// ---- Seed mínimo: crea Persona+Usuario admin si no existe (por correo) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    var correoAdmin = "admin@demo.com";
    var persona = await db.Personas.Include(p => p.Usuario)
                    .FirstOrDefaultAsync(p => p.CorreoElectronico == correoAdmin);

    if (persona == null)
    {
        persona = new  ViviGest.Api.Models.Persona
        {
            IdPersona = Guid.NewGuid(),
            IdTipoDocumento = 1, // CC (existe en catálogo del DDL)
            NumeroDocumento = "9999999999",
            Nombres = "Admin",
            Apellidos = "Demo",
            CorreoElectronico = correoAdmin
        };
        db.Personas.Add(persona);
        await db.SaveChangesAsync();
    }

    if (persona.Usuario == null)
    {
        var pwdSvc = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        pwdSvc.CreatePasswordHash("123456", out var hash, out var salt);

        var usuario = new ViviGest.Api.Models.Usuario
        {
            IdUsuario = Guid.NewGuid(),
            IdPersona = persona.IdPersona,
            ContrasenaHash = hash,
            ContrasenaSalt = salt,
            Activo = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        // Asegura rol Administrador
        var rolAdmin = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Administrador");
        if (rolAdmin == null)
        {
            // En tu DDL el seed crea 'Administrador' en Rol; por si acaso lo insertamos si no estuviera.
            rolAdmin = new ViviGest.Api.Models.Rol { Nombre = "Administrador" };
            db.Roles.Add(rolAdmin);
            await db.SaveChangesAsync();
        }
        db.UsuarioRoles.Add(new ViviGest.Api.Models.UsuarioRol
        {
            IdUsuario = usuario.IdUsuario,
            IdRol = rolAdmin.IdRol
        });
        await db.SaveChangesAsync();
    }
}

app.Run();
