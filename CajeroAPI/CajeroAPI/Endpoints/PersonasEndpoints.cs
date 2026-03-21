using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace CajeroAPI.Endpoints;

public static class PersonasEndpoints
{
    public static WebApplication MapPersonas(this WebApplication app)
    {
        app.MapPost("/personas", async (CajeroAutomaticoDbContext db, CreatePersonaClienteRequest req) =>
        {
            var conn = db.Database.GetDbConnection();

            // Crear comando ADO.NET para ejecutar el SP y capturar el parámetro OUTPUT
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.sp_CrearPersona";
            cmd.CommandType = CommandType.StoredProcedure;

            // Parámetros de entrada
            var pPrimerNombre = new SqlParameter("@PrimerNombre", SqlDbType.VarChar, 50) { Value = (object)req.PrimerNombre ?? DBNull.Value };
            var pSegundoNombre = new SqlParameter("@SegundoNombre", SqlDbType.VarChar, 50) { Value = (object?)req.SegundoNombre ?? DBNull.Value };
            var pTercerNombre = new SqlParameter("@TercerNombre", SqlDbType.VarChar, 50) { Value = (object?)req.TercerNombre ?? DBNull.Value };
            var pPrimerApellido = new SqlParameter("@PrimerApellido", SqlDbType.VarChar, 50) { Value = (object)req.PrimerApellido ?? DBNull.Value };
            var pSegundoApellido = new SqlParameter("@SegundoApellido", SqlDbType.VarChar, 50) { Value = (object?)req.SegundoApellido ?? DBNull.Value };
            var pTercerApellido = new SqlParameter("@TercerApellido", SqlDbType.VarChar, 50) { Value = (object?)req.TercerApellido ?? DBNull.Value };
            var pFechaNacimiento = new SqlParameter("@FechaNacimiento", SqlDbType.Date) { Value = req.FechaNacimiento };
            var pGenero = new SqlParameter("@Genero", SqlDbType.Char, 1) { Value = (object)req.Genero ?? DBNull.Value };
            var pIdEstadoCivil = new SqlParameter("@IdEstadoCivil", SqlDbType.SmallInt) { Value = req.IdEstadoCivil };

            // Agregar parámetros al comando
            cmd.Parameters.Add(pPrimerNombre);
            cmd.Parameters.Add(pSegundoNombre);
            cmd.Parameters.Add(pTercerNombre);
            cmd.Parameters.Add(pPrimerApellido);
            cmd.Parameters.Add(pSegundoApellido);
            cmd.Parameters.Add(pTercerApellido);
            cmd.Parameters.Add(pFechaNacimiento);
            cmd.Parameters.Add(pGenero);
            cmd.Parameters.Add(pIdEstadoCivil);

            // Abrir conexión si es necesario y ejecutar
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
            
            return Results.NoContent();
        })
        .WithName("CrearPersona")
        .WithOpenApi();

        return app;
    }
}

