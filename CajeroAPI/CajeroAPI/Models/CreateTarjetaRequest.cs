using System;

namespace CajeroAPI.Models;

public class CreateTarjetaRequest
{
    public string NumeroCuenta { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public string Pin { get; set; } = string.Empty;
    public string Ccv { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
}
