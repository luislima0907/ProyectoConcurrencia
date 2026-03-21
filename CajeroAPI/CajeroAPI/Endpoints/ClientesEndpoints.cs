using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class ClientesEndpoints
{
    public static WebApplication MapClientes(this WebApplication app)
    {
        app.MapPost("/clientes", async (CajeroAutomaticoDbContext db, CreateClienteRequest req) =>
        {
            var conn = db.Database.GetDbConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_CrearCliente";
            cmd.CommandType = CommandType.StoredProcedure;

            var pIdPersona = new SqlParameter("@IdPersona", SqlDbType.Int) { Value = req.IdPersona };
            cmd.Parameters.Add(pIdPersona);

            try
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                await cmd.ExecuteNonQueryAsync();

                // Obtener el id creado consultando la tabla (el SP no devuelve OUTPUT)
                using var qcmd = conn.CreateCommand();
                qcmd.CommandText = "SELECT IdCliente FROM Cliente WHERE IdPersona = @IdPersona";
                qcmd.CommandType = CommandType.Text;
                var qp = new SqlParameter("@IdPersona", SqlDbType.Int) { Value = req.IdPersona };
                qcmd.Parameters.Add(qp);

                var result = await qcmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                {
                    return Results.StatusCode(500);
                }

                var idCreado = Convert.ToInt32(result);
                return Results.Created($"/clientes/{idCreado}", new { IdCliente = idCreado });
            }
            catch (SqlException ex) when (ex.Number == 50021 || ex.Number == 50022)
            {
                // Errores lanzados en el SP para casos de validación
                LogDeadlock("CrearCliente", $"IdPersona:{req.IdPersona}", ex);
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CrearCliente")
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

