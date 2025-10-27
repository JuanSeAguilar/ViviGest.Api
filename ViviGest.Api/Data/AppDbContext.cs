using Microsoft.EntityFrameworkCore;
using ViviGest.Api.Models;

namespace ViviGest.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Núcleo identidad/personas
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    // Inmobiliario
    public DbSet<Conjunto> Conjuntos => Set<Conjunto>();
    public DbSet<Torre> Torres => Set<Torre>();
    public DbSet<Unidad> Unidades => Set<Unidad>();
    public DbSet<Residencia> Residencias => Set<Residencia>();

    // Operación de portería
    public DbSet<Visitante> Visitantes => Set<Visitante>();
    public DbSet<Visita> Visitas => Set<Visita>();
    public DbSet<PersonaAutorizada> PersonasAutorizadas => Set<PersonaAutorizada>();
    public DbSet<Correspondencia> Correspondencias => Set<Correspondencia>();

    // Catálogos (déjalos solo si SON tablas en tu modelo, no enums)
    // public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    // public DbSet<TipoCorrespondencia> TiposCorrespondencia => Set<TipoCorrespondencia>();
    // public DbSet<EstadoCorrespondencia> EstadosCorrespondencia => Set<EstadoCorrespondencia>();
    // public DbSet<TipoRelacionAutorizado> TiposRelacionAutorizado => Set<TipoRelacionAutorizado>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ===== Persona
        mb.Entity<Persona>(e =>
        {
            e.ToTable("Persona");
            e.HasKey(x => x.IdPersona);
            e.Property(x => x.IdPersona).HasColumnName("IdPersona");
            e.Property(x => x.CorreoElectronico).HasMaxLength(120);
            e.HasIndex(x => x.CorreoElectronico).IsUnique(); // según DDL
            // Si usas catálogo como tabla:
            // e.HasOne(x => x.TipoDocumentoNav).WithMany().HasForeignKey(x => x.IdTipoDocumento);
        });

        // ===== Usuario
        mb.Entity<Usuario>(e =>
        {
            e.ToTable("Usuario");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.ContrasenaHash).HasColumnType("varbinary(256)");
            e.Property(x => x.ContrasenaSalt).HasColumnType("varbinary(128)");
            e.HasOne(x => x.Persona)
             .WithOne(p => p.Usuario)
             .HasForeignKey<Usuario>(x => x.IdPersona);
        });

        // ===== Rol (catálogo)
        mb.Entity<Rol>(e =>
        {
            e.ToTable("Rol");
            e.HasKey(x => x.IdRol);
            e.Property(x => x.Nombre).HasMaxLength(30);
        });

        // ===== UsuarioRol (N-N)
        mb.Entity<UsuarioRol>(e =>
        {
            e.ToTable("UsuarioRol");
            e.HasKey(x => new { x.IdUsuario, x.IdRol });
            e.HasOne(x => x.Usuario)
             .WithMany(u => u.UsuarioRoles)
             .HasForeignKey(x => x.IdUsuario);
            e.HasOne(x => x.Rol)
             .WithMany(r => r.UsuarioRoles)
             .HasForeignKey(x => x.IdRol);
        });

        // ===== Conjunto
        mb.Entity<Conjunto>(e =>
        {
            e.ToTable("Conjunto");
            e.HasKey(x => x.IdConjunto);
        });

        // ===== Torre
        mb.Entity<Torre>(e =>
        {
            e.ToTable("Torre");
            e.HasKey(x => x.IdTorre);
            e.HasOne(x => x.Conjunto)
             .WithMany(c => c.Torres)
             .HasForeignKey(x => x.IdConjunto);
            e.HasIndex(x => new { x.IdConjunto, x.Nombre }).IsUnique(); // UQ_Torre
        });

        // ===== Unidad  (¡clave primaria explícita!)
        mb.Entity<Unidad>(e =>
        {
            e.ToTable("Unidad");
            e.HasKey(x => x.IdUnidad); // <- evita el error "requires a primary key"
            e.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Torre)
             .WithMany(t => t.Unidades)
             .HasForeignKey(x => x.IdTorre);
            e.HasIndex(x => new { x.IdTorre, x.Codigo }).IsUnique(); // UQ_Unidad
        });

        // ===== Residencia (histórico Usuario-Unidad)
        mb.Entity<Residencia>(e =>
        {
            e.ToTable("Residencia");
            e.HasKey(x => x.IdResidencia);
            e.HasOne(x => x.Usuario).WithMany() // si quieres nav inversa, añádela en Usuario
             .HasForeignKey(x => x.IdUsuario);
            e.HasOne(x => x.Unidad)
             .WithMany(u => u.Residencias)
             .HasForeignKey(x => x.IdUnidad);
            e.HasIndex(x => new { x.IdUsuario, x.IdUnidad, x.FechaInicio }).IsUnique(); // UQ_Residencia
        });

        // ===== Visitante (1-1 con Persona en tu DDL)
        mb.Entity<Visitante>(e =>
        {
            e.ToTable("Visitante");
            e.HasKey(x => x.IdVisitante);
            e.HasOne(x => x.Persona)
             .WithOne()
             .HasForeignKey<Visitante>(x => x.IdPersona);
        });

        // ===== Visita
        mb.Entity<Visita>(e =>
        {
            e.ToTable("Visita");
            e.HasKey(x => x.IdVisita);
            e.HasOne(x => x.Visitante)
             .WithMany()
             .HasForeignKey(x => x.IdVisitante);
            e.HasOne(x => x.Unidad)
             .WithMany(u => u.Visitas)
             .HasForeignKey(x => x.IdUnidad);
            e.HasOne(x => x.UsuarioRegistro)
             .WithMany()
             .HasForeignKey(x => x.IdUsuarioRegistro);
            e.HasIndex(x => new { x.IdUnidad, x.FechaRegistro });
        });

        // ===== PersonaAutorizada
        mb.Entity<PersonaAutorizada>(e =>
        {
            e.ToTable("PersonaAutorizada");
            e.HasKey(x => x.IdPersonaAutorizada);
            e.HasOne(x => x.UsuarioResidente)
             .WithMany()
             .HasForeignKey(x => x.IdUsuarioResidente);
            e.HasOne(x => x.Persona)
             .WithMany()
             .HasForeignKey(x => x.IdPersona);
            // Si TipoRelacion es catálogo-tabla:
            // e.HasOne(x => x.TipoRelacionNav).WithMany().HasForeignKey(x => x.IdTipoRelacionAutorizado);
            e.HasIndex(x => new { x.IdUsuarioResidente, x.IdPersona }).IsUnique();
        });

        // ===== Correspondencia
        mb.Entity<Correspondencia>(e =>
        {
            e.ToTable("Correspondencia");
            e.HasKey(x => x.IdCorrespondencia);
            e.HasOne(x => x.Unidad)
             .WithMany(u => u.Correspondencias)
             .HasForeignKey(x => x.IdUnidad);
            // Si usas catálogos como tablas:
            // e.HasOne(x => x.TipoCorrespondenciaNav).WithMany().HasForeignKey(x => x.IdTipoCorrespondencia);
            // e.HasOne(x => x.EstadoCorrespondenciaNav).WithMany().HasForeignKey(x => x.IdEstadoCorrespondencia);
            e.HasOne(x => x.UsuarioRegistro)
             .WithMany()
             .HasForeignKey(x => x.IdUsuarioRegistro);
            e.HasIndex(x => new { x.IdUnidad, x.FechaRecepcion });
        });

        // === Catálogos como tablas (solo si NO son enums en C#) ===
        // mb.Entity<TipoDocumento>().ToTable("TipoDocumento").HasKey(x => x.IdTipoDocumento);
        // mb.Entity<TipoCorrespondencia>().ToTable("TipoCorrespondencia").HasKey(x => x.IdTipoCorrespondencia);
        // mb.Entity<EstadoCorrespondencia>().ToTable("EstadoCorrespondencia").HasKey(x => x.IdEstadoCorrespondencia);
        // mb.Entity<TipoRelacionAutorizado>().ToTable("TipoRelacionAutorizado").HasKey(x => x.IdTipoRelacionAutorizado);
    }
}
