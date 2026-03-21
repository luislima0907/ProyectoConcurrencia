using System.Data;
using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class CuentasEndpoints
{
    public static WebApplication MapCuentas(this WebApplication app)
    {
        app.MapPost("/cuentas", async (CajeroAutomaticoDbContext db, CreateCuentaRequest req) =>
        {
            var parametros = new SqlParameter[]
            {
                new SqlParameter("@IdCliente", SqlDbType.Int) { Value = req.IdCliente },
                new SqlParameter("@IdTipoServicio", SqlDbType.SmallInt) { Value = req.IdTipoServicio },
                new SqlParameter("@SaldoInicial", SqlDbType.Decimal) { Value = req.SaldoInicial, Precision = 18, Scale = 2 }
            };

            await db.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_CrearCuenta @IdCliente, @IdTipoServicio, @SaldoInicial",
                parametros.Cast<object>().ToArray());

            return Results.NoContent();
        })
        .WithName("CrearCuenta")
        .WithOpenApi();

        return app;
    }
}

