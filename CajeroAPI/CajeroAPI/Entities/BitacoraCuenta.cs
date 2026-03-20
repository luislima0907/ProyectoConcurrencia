using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

[PrimaryKey("IdBitacora", "IdMovimiento")]
public partial class BitacoraCuenta
{
    [Key]
    public int IdBitacora { get; set; }

    [Key]
    public int IdMovimiento { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Estado { get; set; } = null!;

    [ForeignKey("IdMovimiento")]
    [InverseProperty("BitacoraCuenta")]
    public virtual MovimientoCuenta IdMovimientoNavigation { get; set; } = null!;
}
