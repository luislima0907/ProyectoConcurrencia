using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class EstadoCivil
{
    [Key]
    public short IdEstadoCivil { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("IdEstadoCivilNavigation")]
    public virtual ICollection<Persona> Persona { get; set; } = new List<Persona>();
}
