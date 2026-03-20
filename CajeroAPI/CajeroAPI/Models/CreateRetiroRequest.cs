using System;

namespace CajeroAPI.Models;

public class CreateRetiroRequest
{
    public string NumeroCuenta { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public decimal Monto { get; set; }
    public string? NumeroTarjeta { get; set; }
    public string? Pin { get; set; }
}

