using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

[Index("IdPersona", Name = "UQ__Cliente__2EC8D2ADA98178AB", IsUnique = true)]
public partial class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    public int IdPersona { get; set; }

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Cuenta> Cuenta { get; set; } = new List<Cuenta>();

    [InverseProperty("IdClienteNavigation")]
    public virtual ICollection<Direccion> Direccion { get; set; } = new List<Direccion>();

    [ForeignKey("IdPersona")]
    [InverseProperty("Cliente")]
    public virtual Persona IdPersonaNavigation { get; set; } = null!;
}
