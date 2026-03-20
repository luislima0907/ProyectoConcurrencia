using System;

namespace CajeroAPI.Models;

public class CreateChequeRequest
{
    public string NumeroCuenta { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaCheque { get; set; }
    public int IdPersonaCobra { get; set; }
}

