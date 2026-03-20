using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class TipoServicioFinanciero
{
    [Key]
    public short IdTipoServicio { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("IdTipoServicioNavigation")]
    public virtual ICollection<Cuenta> Cuenta { get; set; } = new List<Cuenta>();

    [InverseProperty("IdTipoServicioNavigation")]
    public virtual ICollection<Tarjeta> Tarjeta { get; set; } = new List<Tarjeta>();
}
