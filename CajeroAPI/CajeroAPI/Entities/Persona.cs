using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class Persona
{
    [Key]
    public int IdPersona { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PrimerNombre { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? SegundoNombre { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TercerNombre { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string PrimerApellido { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? SegundoApellido { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? TercerApellido { get; set; }

    public DateOnly FechaNacimiento { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string Genero { get; set; } = null!;

    public short IdEstadoCivil { get; set; }

    [InverseProperty("IdPersonaNavigation")]
    public virtual Cliente? Cliente { get; set; }

    [ForeignKey("IdEstadoCivil")]
    [InverseProperty("Persona")]
    public virtual EstadoCivil IdEstadoCivilNavigation { get; set; } = null!;

    [InverseProperty("IdPersonaNavigation")]
    public virtual ICollection<MovimientoCuenta> MovimientoCuenta { get; set; } = new List<MovimientoCuenta>();

    [InverseProperty("IdPersonaNavigation")]
    public virtual ICollection<PersonaTelefono> PersonaTelefono { get; set; } = new List<PersonaTelefono>();
}
