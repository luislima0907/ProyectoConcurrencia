using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class Tarjeta
{
    [Key]
    [StringLength(20)]
    [Unicode(false)]
    public string NumeroTarjeta { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string NumeroCuenta { get; set; } = null!;

    public int IdCliente { get; set; }

    public short IdTipoServicio { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Estado { get; set; } = null!;

    [StringLength(4)]
    [Unicode(false)]
    public string PIN { get; set; } = null!;

    [StringLength(3)]
    [Unicode(false)]
    public string CCV { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaExpiracion { get; set; }

    [ForeignKey("NumeroCuenta, IdCliente")]
    [InverseProperty("Tarjeta")]
    public virtual Cuenta Cuenta { get; set; } = null!;

    [ForeignKey("IdTipoServicio")]
    [InverseProperty("Tarjeta")]
    public virtual TipoServicioFinanciero IdTipoServicioNavigation { get; set; } = null!;

    [InverseProperty("NumeroTarjetaNavigation")]
    public virtual ICollection<MovimientoCuenta> MovimientoCuenta { get; set; } = new List<MovimientoCuenta>();
}
