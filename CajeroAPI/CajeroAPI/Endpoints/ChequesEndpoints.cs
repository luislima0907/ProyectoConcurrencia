using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class ChequesEndpoints
{
    public static WebApplication MapCheques(this WebApplication app)
    {
        app.MapPost("/cheques", async (CajeroAutomaticoDbContext db, CreateChequeRequest req) =>
        {
            // Log de parámetros para depuración
            Console.WriteLine($"[PagarCheque] Params -> NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, FechaCheque:{req.FechaCheque}, IdPersonaCobra:{req.IdPersonaCobra}");

            var conn = db.Database.GetDbConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_PagarCheque";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@NumeroCuenta", SqlDbType.Char, 20) { Value = req.NumeroCuenta });
            cmd.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int) { Value = req.IdCliente });
            var pMonto = new SqlParameter("@Monto", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = req.Monto };
            cmd.Parameters.Add(pMonto);
            cmd.Parameters.Add(new SqlParameter("@FechaCheque", SqlDbType.Date) { Value = req.FechaCheque });
            cmd.Parameters.Add(new SqlParameter("@IdPersonaCobra", SqlDbType.Int) { Value = req.IdPersonaCobra });

            try
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                string mensaje = string.Empty;
                if (await reader.ReadAsync())
                {
                    // El SP selecciona un mensaje como 'Cheque pagado con éxito' en caso de éxito
                    mensaje = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                }

                Console.WriteLine($"[PagarCheque] SP returned message: {mensaje}");

                return Results.Ok(new { Resultado = mensaje });
            }
            catch (SqlException ex)
            {
                // Log deadlock si corresponde
                LogDeadlock("PagarCheque", $"NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, FechaCheque:{req.FechaCheque}, IdPersonaCobra:{req.IdPersonaCobra}", ex);
                return Results.BadRequest(new { error = ex.Message, code = ex.Number });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("PagarCheque")
        .WithOpenApi();

        return app;
    }

    private static void LogDeadlock(string endpoint, string paramInfo, SqlException ex)
    {
        if (ex != null && ex.Number == 1205)
        {
            Console.WriteLine($"[{endpoint}] DEADLOCK detected. Params -> {paramInfo}. SqlException.Number={ex.Number} Message={ex.Message}");
        }
    }
}
