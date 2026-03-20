using System;

namespace CajeroAPI.Models;

public class CreatePersonaClienteRequest
{
    public string PrimerNombre { get; set; } = string.Empty;
    public string? SegundoNombre { get; set; }
    public string? TercerNombre { get; set; }
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string? TercerApellido { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty; // 'M' o 'F'
    public short IdEstadoCivil { get; set; }
}

