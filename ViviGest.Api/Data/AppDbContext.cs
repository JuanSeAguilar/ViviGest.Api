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

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Persona
        mb.Entity<Persona>(e =>
        {
            e.ToTable("Persona");
            e.HasKey(x => x.IdPersona);
            e.Property(x => x.IdPersona).HasColumnName("IdPersona");
            e.Property(x => x.CorreoElectronico).HasMaxLength(120);
            e.HasIndex(x => x.CorreoElectronico).IsUnique(); // según DDL
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
    }
}
