using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

[PrimaryKey("IdPersonaTelefono", "IdPersona")]
public partial class PersonaTelefono
{
    [Key]
    public int IdPersonaTelefono { get; set; }

    [Key]
    public int IdPersona { get; set; }

    public short IdTipoTelefono { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Telefono { get; set; } = null!;

    [ForeignKey("IdPersona")]
    [InverseProperty("PersonaTelefono")]
    public virtual Persona IdPersonaNavigation { get; set; } = null!;

    [ForeignKey("IdTipoTelefono")]
    [InverseProperty("PersonaTelefono")]
    public virtual TipoTelefono IdTipoTelefonoNavigation { get; set; } = null!;
}
