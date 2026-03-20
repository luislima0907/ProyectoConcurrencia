using System;
using System.Collections.Generic;
using CajeroAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Context;

public partial class CajeroAutomaticoDbContext : DbContext
{
    public CajeroAutomaticoDbContext()
    {
    }

    public CajeroAutomaticoDbContext(DbContextOptions<CajeroAutomaticoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Banco> Banco { get; set; }

    public virtual DbSet<BitacoraCuenta> BitacoraCuenta { get; set; }

    public virtual DbSet<Cliente> Cliente { get; set; }

    public virtual DbSet<Cuenta> Cuenta { get; set; }

    public virtual DbSet<Departamento> Departamento { get; set; }

    public virtual DbSet<Direccion> Direccion { get; set; }

    public virtual DbSet<EstadoCivil> EstadoCivil { get; set; }

    public virtual DbSet<MovimientoCuenta> MovimientoCuenta { get; set; }

    public virtual DbSet<Municipio> Municipio { get; set; }

    public virtual DbSet<Persona> Persona { get; set; }

    public virtual DbSet<PersonaTelefono> PersonaTelefono { get; set; }

    public virtual DbSet<Tarjeta> Tarjeta { get; set; }

    public virtual DbSet<TipoMovimientoCuenta> TipoMovimientoCuenta { get; set; }

    public virtual DbSet<TipoServicioFinanciero> TipoServicioFinanciero { get; set; }

    public virtual DbSet<TipoTelefono> TipoTelefono { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // Evita sobrescribir la configuración cuando se inyecta el DbContext desde DI
    if (!optionsBuilder.IsConfigured)
    {
        optionsBuilder.UseSqlServer("Server=DESKTOP-9SBPDDD;Database=CajeroAutomatico;User Id=sa;Password=Hell0w0rld3312j$;TrustServerCertificate=True;");
    }
}


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Banco>(entity =>
        {
            entity.HasKey(e => e.IdBanco).HasName("PK__Banco__2D3F553EAE0B8B2A");

            entity.Property(e => e.IdBanco).ValueGeneratedNever();
        });

        modelBuilder.Entity<BitacoraCuenta>(entity =>
        {
            entity.HasKey(e => new { e.IdBitacora, e.IdMovimiento }).HasName("PK_Bitacora");

            entity.Property(e => e.Estado).IsFixedLength();

            entity.HasOne(d => d.IdMovimientoNavigation).WithMany(p => p.BitacoraCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bitacora_Movimiento");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Cliente__D5946642CF83BB56");

            entity.Property(e => e.IdCliente).ValueGeneratedNever();

            entity.HasOne(d => d.IdPersonaNavigation).WithOne(p => p.Cliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cliente_Persona");
        });

        modelBuilder.Entity<Cuenta>(entity =>
        {
            entity.Property(e => e.NumeroCuenta).IsFixedLength();
            entity.Property(e => e.Estado).IsFixedLength();

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cuenta_Cliente");

            entity.HasOne(d => d.IdTipoServicioNavigation).WithMany(p => p.Cuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cuenta_TipoServicioFinanciero");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PK__Departam__787A433DC6360DE8");

            entity.Property(e => e.IdDepartamento).ValueGeneratedNever();
        });

        modelBuilder.Entity<Direccion>(entity =>
        {
            entity.HasKey(e => e.IdDireccion).HasName("PK__Direccio__1F8E0C76BF124F36");

            entity.Property(e => e.IdDireccion).ValueGeneratedNever();

            entity.HasOne(d => d.IdBancoNavigation).WithMany(p => p.Direccion).HasConstraintName("FK_Direccion_Banco");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Direccion).HasConstraintName("FK_Direccion_Cliente");

            entity.HasOne(d => d.Municipio).WithMany(p => p.Direccion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Direccion_Municipio");
        });

        modelBuilder.Entity<EstadoCivil>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCivil).HasName("PK__EstadoCi__889DE1B2EB41FB9A");

            entity.Property(e => e.IdEstadoCivil).ValueGeneratedNever();
        });

        modelBuilder.Entity<MovimientoCuenta>(entity =>
        {
            entity.HasKey(e => e.IdMovimiento).HasName("PK__Movimien__881A6AE0DA0CEE64");

            entity.Property(e => e.IdMovimiento).ValueGeneratedNever();
            entity.Property(e => e.NumeroCuenta).IsFixedLength();
            entity.Property(e => e.NumeroTarjeta).IsFixedLength();
            entity.Property(e => e.UsuarioSistema).HasDefaultValueSql("(suser_sname())");

            entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.MovimientoCuenta).HasConstraintName("FK_Movimiento_Persona");

            entity.HasOne(d => d.IdTipoMovimientoNavigation).WithMany(p => p.MovimientoCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimiento_Tipo");

            entity.HasOne(d => d.NumeroTarjetaNavigation).WithMany(p => p.MovimientoCuenta).HasConstraintName("FK_Movimiento_Tarjeta");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.MovimientoCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Movimiento_Cuenta");
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Municipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Municipio_Departamento");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.HasKey(e => e.IdPersona).HasName("PK__Persona__2EC8D2ACFC262450");

            entity.Property(e => e.IdPersona).ValueGeneratedNever();
            entity.Property(e => e.Genero).IsFixedLength();

            entity.HasOne(d => d.IdEstadoCivilNavigation).WithMany(p => p.Persona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Persona_EstadoCivil");
        });

        modelBuilder.Entity<PersonaTelefono>(entity =>
        {
            entity.HasOne(d => d.IdPersonaNavigation).WithMany(p => p.PersonaTelefono)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PT_Persona");

            entity.HasOne(d => d.IdTipoTelefonoNavigation).WithMany(p => p.PersonaTelefono)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PT_TipoTelefono");
        });

        modelBuilder.Entity<Tarjeta>(entity =>
        {
            entity.HasKey(e => e.NumeroTarjeta).HasName("PK__Tarjeta__BC163C0BED7B5E07");

            entity.Property(e => e.NumeroTarjeta).IsFixedLength();
            entity.Property(e => e.CCV).IsFixedLength();
            entity.Property(e => e.Estado).IsFixedLength();
            entity.Property(e => e.NumeroCuenta).IsFixedLength();
            entity.Property(e => e.PIN).IsFixedLength();

            entity.HasOne(d => d.IdTipoServicioNavigation).WithMany(p => p.Tarjeta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tarjeta_TipoServicioFinanciero");

            entity.HasOne(d => d.Cuenta).WithMany(p => p.Tarjeta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tarjeta_Cuenta");
        });

        modelBuilder.Entity<TipoMovimientoCuenta>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimiento).HasName("PK__TipoMovi__820D7FC2C45FE29B");

            entity.Property(e => e.IdTipoMovimiento).ValueGeneratedNever();
            entity.Property(e => e.Naturaleza).IsFixedLength();
        });

        modelBuilder.Entity<TipoServicioFinanciero>(entity =>
        {
            entity.HasKey(e => e.IdTipoServicio).HasName("PK__TipoServ__E29B3EA72041CB51");

            entity.Property(e => e.IdTipoServicio).ValueGeneratedNever();
        });

        modelBuilder.Entity<TipoTelefono>(entity =>
        {
            entity.HasKey(e => e.IdTipoTelefono).HasName("PK__TipoTele__6FA65CBFD1C0C6E4");

            entity.Property(e => e.IdTipoTelefono).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
