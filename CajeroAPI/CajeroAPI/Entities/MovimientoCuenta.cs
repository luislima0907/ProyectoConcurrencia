using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class MovimientoCuenta
{
    [Key]
    public int IdMovimiento { get; set; }

    public short IdTipoMovimiento { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaDocumento { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string NumeroCuenta { get; set; } = null!;

    public int IdCliente { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? NumeroTarjeta { get; set; }

    public int? IdPersona { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string UsuarioSistema { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Monto { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaHora { get; set; }

    [InverseProperty("IdMovimientoNavigation")]
    public virtual ICollection<BitacoraCuenta> BitacoraCuenta { get; set; } = new List<BitacoraCuenta>();

    [ForeignKey("NumeroCuenta, IdCliente")]
    [InverseProperty("MovimientoCuenta")]
    public virtual Cuenta Cuenta { get; set; } = null!;

    [ForeignKey("IdPersona")]
    [InverseProperty("MovimientoCuenta")]
    public virtual Persona? IdPersonaNavigation { get; set; }

    [ForeignKey("IdTipoMovimiento")]
    [InverseProperty("MovimientoCuenta")]
    public virtual TipoMovimientoCuenta IdTipoMovimientoNavigation { get; set; } = null!;

    [ForeignKey("NumeroTarjeta")]
    [InverseProperty("MovimientoCuenta")]
    public virtual Tarjeta? NumeroTarjetaNavigation { get; set; }
}
