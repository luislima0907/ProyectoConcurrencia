using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class NotasCreditoEndpoints
{
    public static WebApplication MapNotasCredito(this WebApplication app)
    {
        app.MapPost("/notas-credito", async (CajeroAutomaticoDbContext db, CreateNotaCreditoRequest req) =>
        {
            // Log de parámetros para depuración
            Console.WriteLine($"[NotaCredito] Params -> NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, NumeroTarjeta:{req.NumeroTarjeta}, IdPersona:{req.IdPersona}");

            var conn = db.Database.GetDbConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_NotaCredito";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@NumeroCuenta", SqlDbType.Char, 20) { Value = req.NumeroCuenta });
            cmd.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int) { Value = req.IdCliente });
            cmd.Parameters.Add(new SqlParameter("@Monto", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = req.Monto });
            cmd.Parameters.Add(new SqlParameter("@NumeroTarjeta", SqlDbType.Char, 20) { Value = (object?)req.NumeroTarjeta ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@IdPersona", SqlDbType.Int) { Value = (object?)req.IdPersona ?? DBNull.Value });

            try
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var idMovimiento = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                    var tipoOperacion = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var saldoAnterior = reader.IsDBNull(2) ? (decimal?)null : reader.GetDecimal(2);
                    var saldoNuevo = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3);
                    var monto = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4);
                    var fechaHora = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

                    Console.WriteLine($"[NotaCredito] SP success: TipoOperacion={tipoOperacion} IdMovimiento={idMovimiento} Monto={monto} SaldoAnterior={saldoAnterior} SaldoNuevo={saldoNuevo} FechaHora={fechaHora}");

                    return Results.Ok(new
                    {
                        IdMovimiento = idMovimiento,
                        TipoOperacion = tipoOperacion,
                        SaldoAnterior = saldoAnterior,
                        SaldoNuevo = saldoNuevo,
                        Monto = monto,
                        FechaHora = fechaHora
                    });
                }

                // Si no devuelve filas, devolver 400 con mensaje genérico
                return Results.BadRequest(new { error = "El procedimiento no devolvió datos. Verifica parámetros o estado de la cuenta." });
            }
            catch (SqlException ex)
            {
                // Log deadlock si corresponde
                LogDeadlock("NotaCredito", $"NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, NumeroTarjeta:{req.NumeroTarjeta}, IdPersona:{req.IdPersona}", ex);
                return Results.BadRequest(new { error = ex.Message, code = ex.Number });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("NotaCredito")
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
