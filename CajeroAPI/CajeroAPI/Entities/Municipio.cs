using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

[PrimaryKey("IdMunicipio", "IdDepartamento")]
public partial class Municipio
{
    [Key]
    public int IdMunicipio { get; set; }

    [Key]
    public int IdDepartamento { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("Municipio")]
    public virtual ICollection<Direccion> Direccion { get; set; } = new List<Direccion>();

    [ForeignKey("IdDepartamento")]
    [InverseProperty("Municipio")]
    public virtual Departamento IdDepartamentoNavigation { get; set; } = null!;
}
