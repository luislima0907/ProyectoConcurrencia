namespace CajeroAPI.Models;

public class CreateDepositoRequest
{
    public string NumeroCuenta { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public decimal Monto { get; set; }
    public int? IdPersonaQueDeposita { get; set; }
}

