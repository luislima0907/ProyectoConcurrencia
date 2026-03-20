using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Entities;

public partial class Direccion
{
    [Key]
    public int IdDireccion { get; set; }

    public int? IdCliente { get; set; }

    public int? IdBanco { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Calle { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Avenida { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Zona { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Ciudad { get; set; } = null!;

    public int IdDepartamento { get; set; }

    public int IdMunicipio { get; set; }

    [ForeignKey("IdBanco")]
    [InverseProperty("Direccion")]
    public virtual Banco? IdBancoNavigation { get; set; }

    [ForeignKey("IdCliente")]
    [InverseProperty("Direccion")]
    public virtual Cliente? IdClienteNavigation { get; set; }

    [ForeignKey("IdMunicipio, IdDepartamento")]
    [InverseProperty("Direccion")]
    public virtual Municipio Municipio { get; set; } = null!;
}
