using CajeroAPI.Context;
using CajeroAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using CajeroAPI.Endpoints;

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
// Mostrar Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cajero API v1");
    // Servir la UI en la raíz (acceder por /)
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

// Mapeo de endpoints (implementados en clases separadas bajo Endpoints/)
app.MapTarjetas();
app.MapCuentas();
app.MapPersonas();
app.MapClientes();
app.MapDepositos();
app.MapCheques();
app.MapNotasCredito();
app.MapNotasDebito();
app.MapRetiros();

// Agregar endpoint /health simple para comprobaciones de estado
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }))
   .WithName("Health")
   .WithOpenApi();

app.Run();