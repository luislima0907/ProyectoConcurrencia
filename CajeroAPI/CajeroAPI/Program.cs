using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CajeroAutomaticoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CajeroAutomatico")));

builder.Services.AddEndpointsApiExplorer();
// Configuración mínima de Swashbuckle/Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cajero API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Mostrar Swagger UI (en desarrollo o siempre para facilitar pruebas locales)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cajero API v1");
    // Servir la UI en la raíz (acceder por /)
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

// Endpoint to create a tarjeta by calling the stored procedure in the database.
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

    // El procedimiento no devuelve la tarjeta; devolver 204 No Content para indicar éxito.
    return Results.NoContent();
})
.WithName("CrearTarjeta")
.WithOpenApi();

// Endpoint to create a cuenta by calling the stored procedure in the database.
app.MapPost("/cuentas", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreateCuentaRequest req) =>
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

// Endpoint to create persona y cliente by calling the stored procedure in the database and capturing the OUTPUT id.
app.MapPost("/personas", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreatePersonaClienteRequest req) =>
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

// Endpoint to create a cliente by calling the stored procedure in the database.
app.MapPost("/clientes", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreateClienteRequest req) =>
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
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("CrearCliente")
.WithOpenApi();

// Endpoint to perform a deposit using the stored procedure sp_Depositar
app.MapPost("/depositos", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreateDepositoRequest req) =>
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

        return Results.Ok(new { Resultado = mensaje });
    }
    catch (SqlException ex)
    {
        // Reenviar mensajes y códigos SQL al cliente para facilitar depuración
        return Results.BadRequest(new { error = ex.Message, code = ex.Number });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("Depositar")
.WithOpenApi();

// Endpoint to perform a cheque payment using the stored procedure sp_PagarCheque
app.MapPost("/cheques", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreateChequeRequest req) =>
{
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

        return Results.Ok(new { Resultado = mensaje });
    }
    catch (SqlException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.Number });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("PagarCheque")
.WithOpenApi();

// Endpoint to create a nota de crédito using the stored procedure sp_NotaCredito
app.MapPost("/notas-credito", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreateNotaCreditoRequest req) =>
{
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
        return Results.BadRequest(new { error = ex.Message, code = ex.Number });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("NotaCredito")
.WithOpenApi();

// Endpoint to perform a nota de débito using the stored procedure sp_NotaDebito
app.MapPost("/notas-debito", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreateNotaDebitoRequest req) =>
{
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
                return Results.BadRequest(new { error = mensaje });
            }

            // En caso de éxito devuelve 6 columnas: IdMovimiento, TipoOperacion, SaldoAnterior, SaldoNuevo, Monto, FechaHora
            var idMovimiento = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0);
            var tipoOperacion = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var saldoAnterior = reader.IsDBNull(2) ? (decimal?)null : reader.GetDecimal(2);
            var saldoNuevo = reader.IsDBNull(3) ? (decimal?)null : reader.GetDecimal(3);
            var monto = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4);
            var fechaHora = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);

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
        return Results.BadRequest(new { error = ex.Message, code = ex.Number });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("NotaDebito")
.WithOpenApi();

// Endpoint to perform a retiro (withdrawal) using the stored procedure sp_Retirar
app.MapPost("/retiros", async (CajeroAutomaticoDbContext db, CajeroAPI.Models.CreateRetiroRequest req) =>
{
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
                return Results.BadRequest(new { error = mensaje });
            }
        }

        // No hay filas devueltas: el procedimiento grabó movimiento con estado no exitoso; devolver 204
        return Results.NoContent();
    }
    catch (SqlException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.Number });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
})
.WithName("Retirar")
.WithOpenApi();

app.Run();