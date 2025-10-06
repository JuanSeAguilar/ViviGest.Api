using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ViviGest.Api.Models;
using ViviGest.Api.Services;
using ViviGest.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("=== 🔧 DEBUG CONFIGURACIÓN ===");
Console.WriteLine($"Configuration Sources: {string.Join(", ", builder.Configuration.Sources.Select(s => s.GetType().Name))}");

// Verificar si existe la sección Jwt
var jwtSection = builder.Configuration.GetSection("Jwt");
Console.WriteLine($"Jwt Section exists: {jwtSection.Exists()}");

// Listar todas las configuraciones disponibles
var allConfigs = builder.Configuration.AsEnumerable();
Console.WriteLine("Available configurations:");
foreach (var config in allConfigs)
{
    if (!string.IsNullOrEmpty(config.Value))
    {
        Console.WriteLine($"  {config.Key} = {config.Value}");
    }
}
Console.WriteLine("=== FIN DEBUG ===");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordService, PasswordService>();

// 👇 CORS CORREGIDO - AGREGAR PUERTO 5171
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("FrontPolicy", p => p
        .WithOrigins("http://localhost:5171", "http://localhost:5173", "http://localhost:3000") // ✅ 5171 agregado
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // ✅ AllowCredentials agregado
});

// 👇 CONFIGURACIÓN JWT CORREGIDA (SOLO UNA VEZ)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        // 👇 ESTE DEBUG SÍ SE EJECUTA (está dentro del JWT config)
        var jwtKey = builder.Configuration["Jwt:Key"];
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];

        Console.WriteLine("=== 🔐 JWT CONFIGURATION ===");
        Console.WriteLine($"   Key: {(!string.IsNullOrEmpty(jwtKey) ? "PRESENTE" : "FALTANTE")}");
        Console.WriteLine($"   Key value: {jwtKey?.Substring(0, Math.Min(20, jwtKey.Length))}...");
        Console.WriteLine($"   Issuer: {jwtIssuer}");
        Console.WriteLine($"   Audience: {jwtAudience}");
        Console.WriteLine("=== FIN JWT CONFIG ===");

        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new Exception("JWT Key no configurada en appsettings.json");
        }

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // 👇 DEBUG DE EVENTOS JWT
        o.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"🔐 JWT Authentication Failed: {context.Exception.Message}");
                Console.WriteLine($"🔐 Exception: {context.Exception}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"🎉 JWT Token Validated for: {context.Principal?.Identity?.Name}");
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
                Console.WriteLine($"🔐 Claims: {string.Join(", ", claims ?? [])}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                Console.WriteLine($"📨 JWT Token Received: {context.Token?.Substring(0, Math.Min(30, context.Token?.Length ?? 0))}...");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"🚨 JWT Challenge: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ViviGest API", Version = "v1" });

    // Config JWT
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
    c.RoutePrefix = "swagger";
});

app.MapControllers();

// ---- Seed mínimo ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();

    var correoAdmin = "admin@demo.com";
    var persona = await db.Personas.Include(p => p.Usuario)
                    .FirstOrDefaultAsync(p => p.CorreoElectronico == correoAdmin);

    if (persona == null)
    {
        persona = new ViviGest.Api.Models.Persona
        {
            IdPersona = Guid.NewGuid(),
            IdTipoDocumento = 1,
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

app.Run("http://localhost:5170");