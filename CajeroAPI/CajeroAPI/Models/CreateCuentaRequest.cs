namespace CajeroAPI.Models;

public class CreateCuentaRequest
{
    public int IdCliente { get; set; }
    public short IdTipoServicio { get; set; }
    public decimal SaldoInicial { get; set; }
}

