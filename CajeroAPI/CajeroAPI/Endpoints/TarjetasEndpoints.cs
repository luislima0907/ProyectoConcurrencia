using System.Data;
using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class TarjetasEndpoints
{
    public static WebApplication MapTarjetas(this WebApplication app)
    {
        app.MapPost("/tarjetas", async (CajeroAutomaticoDbContext db, CreateTarjetaRequest req) =>
        {
            // Parametrizar la llamada para evitar inyección SQL
            var parametros = new SqlParameter[]
            {
                new SqlParameter("@NumeroCuenta", SqlDbType.Char, 20) { Value = req.NumeroCuenta },
                new SqlParameter("@IdCliente", SqlDbType.Int) { Value = req.IdCliente },
                new SqlParameter("@PIN", SqlDbType.Char, 4) { Value = req.Pin },
                new SqlParameter("@CCV", SqlDbType.Char, 3) { Value = req.Ccv },
                new SqlParameter("@FechaExpiracion", SqlDbType.Date) { Value = req.FechaExpiracion }
            };

            // Ejecutar el procedimiento almacenado (pasar object[] evitando conversión peligrosa)
            await db.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_creartarjetaDebito @NumeroCuenta, @IdCliente, @PIN, @CCV, @FechaExpiracion",
                parametros.Cast<object>().ToArray());

            // Log de éxito
            Console.WriteLine($"[CrearTarjeta] Tarjeta creada para NumeroCuenta:'{req.NumeroCuenta}', IdCliente:{req.IdCliente}");

            // El procedimiento no devuelve la tarjeta; devolver 204 No Content para indicar éxito.
            return Results.NoContent();
        })
        .WithName("CrearTarjeta")
        .WithOpenApi();

        return app;
    }
}
