using Microsoft.EntityFrameworkCore;
using ViviGest.Api.Models;

namespace ViviGest.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();

    // NUEVOS DbSets para correspondencia
    public DbSet<Correspondencia> Correspondencias => Set<Correspondencia>();
    public DbSet<TipoCorrespondencia> TiposCorrespondencia => Set<TipoCorrespondencia>();
    public DbSet<EstadoCorrespondencia> EstadosCorrespondencia => Set<EstadoCorrespondencia>();
    public DbSet<Unidad> Unidades => Set<Unidad>();
    public DbSet<Torre> Torres => Set<Torre>();
    public DbSet<Conjunto> Conjuntos => Set<Conjunto>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Persona
        mb.Entity<Persona>(e =>
        {
            e.ToTable("Persona");
            e.HasKey(x => x.IdPersona);
            e.Property(x => x.IdPersona).HasColumnName("IdPersona");
            e.Property(x => x.CorreoElectronico).HasMaxLength(120);
            e.HasIndex(x => x.CorreoElectronico).IsUnique();
        });

        // Usuario
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

        // Rol (catálogo)
        mb.Entity<Rol>(e =>
        {
            e.ToTable("Rol");
            e.HasKey(x => x.IdRol);
            e.Property(x => x.Nombre).HasMaxLength(30);
        });

        // UsuarioRol (N-N)
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

        // NUEVAS configuraciones para correspondencia
        // Correspondencia
        mb.Entity<Correspondencia>(e =>
        {
            e.ToTable("Correspondencia");
            e.HasKey(x => x.IdCorrespondencia);
            e.Property(x => x.Remitente).HasMaxLength(120);
            e.Property(x => x.Observacion).HasMaxLength(250);
            e.Property(x => x.EntregadoA).HasMaxLength(120);

            e.HasOne(x => x.Unidad)
             .WithMany()
             .HasForeignKey(x => x.IdUnidad);

            e.HasOne(x => x.TipoCorrespondencia)
             .WithMany()
             .HasForeignKey(x => x.IdTipoCorrespondencia);

            e.HasOne(x => x.EstadoCorrespondencia)
             .WithMany()
             .HasForeignKey(x => x.IdEstadoCorrespondencia);

            e.HasOne(x => x.UsuarioRegistro)
             .WithMany()
             .HasForeignKey(x => x.IdUsuarioRegistro);
        });

        // TipoCorrespondencia (catálogo)
        mb.Entity<TipoCorrespondencia>(e =>
        {
            e.ToTable("TipoCorrespondencia");
            e.HasKey(x => x.IdTipoCorrespondencia);
            e.Property(x => x.Nombre).HasMaxLength(30);
        });

        // EstadoCorrespondencia (catálogo)
        mb.Entity<EstadoCorrespondencia>(e =>
        {
            e.ToTable("EstadoCorrespondencia");
            e.HasKey(x => x.IdEstadoCorrespondencia);
            e.Property(x => x.Nombre).HasMaxLength(30);
        });

        // Unidad
        mb.Entity<Unidad>(e =>
        {
            e.ToTable("Unidad");
            e.HasKey(x => x.IdUnidad);
            e.Property(x => x.Codigo).HasMaxLength(20);
            e.Property(x => x.AreaM2).HasColumnType("decimal(8,2)");

            e.HasOne(x => x.Torre)
             .WithMany()
             .HasForeignKey(x => x.IdTorre);
        });

        // Torre
        mb.Entity<Torre>(e =>
        {
            e.ToTable("Torre");
            e.HasKey(x => x.IdTorre);
            e.Property(x => x.Nombre).HasMaxLength(50);

            e.HasOne(x => x.Conjunto)
             .WithMany()
             .HasForeignKey(x => x.IdConjunto);
        });

        // Conjunto
        mb.Entity<Conjunto>(e =>
        {
            e.ToTable("Conjunto");
            e.HasKey(x => x.IdConjunto);
            e.Property(x => x.Nombre).HasMaxLength(120);
        });
    }
}