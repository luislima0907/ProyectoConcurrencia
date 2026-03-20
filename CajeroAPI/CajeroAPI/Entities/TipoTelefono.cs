using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class TipoTelefono
{
    [Key]
    public short IdTipoTelefono { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("IdTipoTelefonoNavigation")]
    public virtual ICollection<PersonaTelefono> PersonaTelefono { get; set; } = new List<PersonaTelefono>();
}
