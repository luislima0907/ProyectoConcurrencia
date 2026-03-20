using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class Departamento
{
    [Key]
    public int IdDepartamento { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("IdDepartamentoNavigation")]
    public virtual ICollection<Municipio> Municipio { get; set; } = new List<Municipio>();
}
