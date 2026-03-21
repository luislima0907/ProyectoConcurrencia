using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class NotasDebitoEndpoints
{
    public static WebApplication MapNotasDebito(this WebApplication app)
    {
        app.MapPost("/notas-debito", async (CajeroAutomaticoDbContext db, CreateNotaDebitoRequest req) =>
        {
            // Log de parámetros para depuración
            Console.WriteLine($"[NotaDebito] Params -> NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, NumeroTarjeta:{req.NumeroTarjeta}");

            var conn = db.Database.GetDbConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_NotaDebito";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@NumeroCuenta", SqlDbType.Char, 20) { Value = req.NumeroCuenta });
            cmd.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int) { Value = req.IdCliente });
            cmd.Parameters.Add(new SqlParameter("@Monto", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = req.Monto });
            cmd.Parameters.Add(new SqlParameter("@NumeroTarjeta", SqlDbType.Char, 20) { Value = req.NumeroTarjeta });

            try
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // El SP devuelve en ramas de error un SELECT con una sola columna mensaje
                    if (reader.FieldCount == 1)
                    {
                        var mensaje = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        Console.WriteLine($"[NotaDebito] SP returned message: {mensaje}");
                        return Results.BadRequest(new { error = mensaje });
                    }

                    // En caso de éxito devuelve 6 columnas: IdMovimiento, TipoOperacion, SaldoAnterior, SaldoNuevo, Monto, FechaHora
                    var idMovimiento = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                    var tipoOperacion = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    var saldoAnterior = reader.IsDBNull(2) ? (decimal?)null : reader.GetDecimal(2);
                    var saldoNuevo = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3);
                    var monto = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4);
                    var fechaHora = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

                    Console.WriteLine($"[NotaDebito] SP success: TipoOperacion={tipoOperacion} IdMovimiento={idMovimiento} Monto={monto} SaldoAnterior={saldoAnterior} SaldoNuevo={saldoNuevo} FechaHora={fechaHora}");
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

                return Results.BadRequest(new { error = "El procedimiento no devolvió datos." });
            }
            catch (SqlException ex)
            {
                // Log deadlock si corresponde
                LogDeadlock("NotaDebito", $"NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, NumeroTarjeta:{req.NumeroTarjeta}", ex);
                return Results.BadRequest(new { error = ex.Message, code = ex.Number });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("NotaDebito")
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
