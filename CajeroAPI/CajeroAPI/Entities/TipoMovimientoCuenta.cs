using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class TipoMovimientoCuenta
{
    [Key]
    public short IdTipoMovimiento { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [StringLength(1)]
    [Unicode(false)]
    public string Naturaleza { get; set; } = null!;

    [InverseProperty("IdTipoMovimientoNavigation")]
    public virtual ICollection<MovimientoCuenta> MovimientoCuenta { get; set; } = new List<MovimientoCuenta>();
}
