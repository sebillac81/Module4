using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoastingManager.Api.Configuracion;
using RoastingManager.Api.Endpoints;
using RoastingManager.Api.ErrorHandling;
using RoastingManager.Infrastructure;
using RoastingManager.Infrastructure.Operaciones;
using RoastingManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Principio VI: la connection string llega por variable de entorno
// (ConnectionStrings__RoastingManager) o user-secrets, nunca desde appsettings.
var connectionString = builder.Configuration.GetConnectionString("RoastingManager")
    ?? throw new InvalidOperationException(
        "Falta la connection string 'RoastingManager'. Definí la variable de entorno " +
        "ConnectionStrings__RoastingManager (ver quickstart.md).");

builder.Services.AddDbContext<RoastingDbContext>(o => o
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention());
builder.Services.Configure<TopesOptions>(builder.Configuration.GetSection(TopesOptions.Seccion));
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.Seccion));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IReloj, RelojSistema>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddScoped<RegistrarIngreso>();
builder.Services.AddScoped<RegistrarBatch>();
builder.Services.AddScoped<RegistrarVenta>();

var app = builder.Build();

Migrador.Ejecutar(connectionString);

app.UseExceptionHandler();
app.UseStatusCodePages();

var api = app.MapGroup("/api");
api.MapGet("/config/topes", (IOptions<TopesOptions> topes) => topes.Value);
api.MapCatalogoCafe();
api.MapIngresos();
api.MapBatches();
api.MapStock();
api.MapCatalogoVenta();
api.MapVentas();

app.Run();

public partial class Program;
