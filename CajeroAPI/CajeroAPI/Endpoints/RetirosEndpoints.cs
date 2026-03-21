using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class RetirosEndpoints
{
    public static WebApplication MapRetiros(this WebApplication app)
    {
        app.MapPost("/retiros", async (CajeroAutomaticoDbContext db, CreateRetiroRequest req) =>
        {
            // Log de parámetros para depuración
            Console.WriteLine($"[Retirar] Params -> NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, NumeroTarjeta:{req.NumeroTarjeta}, PIN:{req.Pin}");

            var conn = db.Database.GetDbConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_Retirar";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@NumeroCuenta", SqlDbType.Char, 20) { Value = req.NumeroCuenta });
            cmd.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int) { Value = req.IdCliente });
            cmd.Parameters.Add(new SqlParameter("@Monto", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = req.Monto });
            cmd.Parameters.Add(new SqlParameter("@NumeroTarjeta", SqlDbType.Char, 20) { Value = (object?)req.NumeroTarjeta ?? DBNull.Value });
            cmd.Parameters.Add(new SqlParameter("@PIN", SqlDbType.Char, 4) { Value = (object?)req.Pin ?? DBNull.Value });

            try
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // El SP, en caso de éxito, devuelve una fila con 6 columnas
                    if (reader.FieldCount >= 6)
                    {
                        var idMovimiento = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
                        var tipoOperacion = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        var saldoAnterior = reader.IsDBNull(2) ? (decimal?)null : reader.GetDecimal(2);
                        var saldoNuevo = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3);
                        var monto = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4);
                        var fechaHora = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

                        Console.WriteLine($"[Retirar] SP success: TipoOperacion={tipoOperacion} IdMovimiento={idMovimiento} Monto={monto} SaldoAnterior={saldoAnterior} SaldoNuevo={saldoNuevo} FechaHora={fechaHora}");

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

                    // Si devuelve una sola columna (mensaje), devolverlo como BadRequest
                    if (reader.FieldCount == 1)
                    {
                        var mensaje = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        Console.WriteLine($"[Retirar] SP returned message: {mensaje}");
                        return Results.BadRequest(new { error = mensaje });
                    }
                }

                // No hay filas devueltas: el procedimiento grabó movimiento con estado no exitoso; devolver 204
                return Results.NoContent();
            }
            catch (SqlException ex)
            {
                // Log deadlock si corresponde
                LogDeadlock("Retirar", $"NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}, Monto:{req.Monto}, NumeroTarjeta:{req.NumeroTarjeta}, PIN:{req.Pin}", ex);
                return Results.BadRequest(new { error = ex.Message, code = ex.Number });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("Retirar")
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
