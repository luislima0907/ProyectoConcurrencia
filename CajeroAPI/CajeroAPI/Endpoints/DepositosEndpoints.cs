using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class DepositosEndpoints
{
    public static WebApplication MapDepositos(this WebApplication app)
    {
        app.MapPost("/depositos", async (CajeroAutomaticoDbContext db, CreateDepositoRequest req) =>
        {
            var conn = db.Database.GetDbConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_Depositar";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@NumeroCuenta", SqlDbType.Char, 20) { Value = req.NumeroCuenta });
            cmd.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int) { Value = req.IdCliente });
            var pMonto = new SqlParameter("@Monto", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = req.Monto };
            cmd.Parameters.Add(pMonto);
            cmd.Parameters.Add(new SqlParameter("@IdPersonaQueDeposita", SqlDbType.Int) { Value = (object?)req.IdPersonaQueDeposita ?? DBNull.Value });

            try
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                string mensaje = string.Empty;
                if (await reader.ReadAsync())
                {
                    // El SP selecciona un mensaje como 'Depósito realizado con éxito'
                    mensaje = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                }

                Console.WriteLine($"[Depositar] SP returned message: {mensaje}");
                return Results.Ok(new { Resultado = mensaje });
            }
            catch (SqlException ex)
            {
                // Loguear deadlock si corresponde
                LogDeadlock("Depositar", $"NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, IdPersonaQueDeposita:{req.IdPersonaQueDeposita}", ex);
                // Reenviar mensajes y códigos SQL al cliente para facilitar depuración
                Console.WriteLine($"[Depositar] SqlException: Number={ex.Number} Message={ex.Message}");
                return Results.BadRequest(new { error = ex.Message, code = ex.Number });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Depositar] Exception: {ex}");
                return Results.Problem(ex.Message);
            }
        })
        .WithName("Depositar")
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
