using System;

namespace CajeroAPI.Models;

public class CreateNotaDebitoRequest
{
    public string NumeroCuenta { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public decimal Monto { get; set; }
    public string NumeroTarjeta { get; set; } = string.Empty;
}

