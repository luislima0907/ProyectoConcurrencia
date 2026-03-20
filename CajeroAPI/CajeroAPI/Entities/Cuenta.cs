using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

[PrimaryKey("NumeroCuenta", "IdCliente")]
public partial class Cuenta
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string NumeroCuenta { get; set; } = null!;

    [Key]
    public int IdCliente { get; set; }

    public short IdTipoServicio { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Estado { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Saldo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaApertura { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCierre { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Cuenta")]
    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    [ForeignKey("IdTipoServicio")]
    [InverseProperty("Cuenta")]
    public virtual TipoServicioFinanciero IdTipoServicioNavigation { get; set; } = null!;

    [InverseProperty("Cuenta")]
    public virtual ICollection<MovimientoCuenta> MovimientoCuenta { get; set; } = new List<MovimientoCuenta>();

    [InverseProperty("Cuenta")]
    public virtual ICollection<Tarjeta> Tarjeta { get; set; } = new List<Tarjeta>();
}
