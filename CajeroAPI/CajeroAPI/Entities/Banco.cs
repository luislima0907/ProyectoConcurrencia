using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class Banco
{
    [Key]
    public int IdBanco { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("IdBancoNavigation")]
    public virtual ICollection<Direccion> Direccion { get; set; } = new List<Direccion>();
}
