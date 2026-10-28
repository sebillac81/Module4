using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using RoastingManager.Api.Configuracion;
using Testcontainers.PostgreSql;

namespace RoastingManager.Api.Tests.Infra;

/// <summary>Reloj fijo para que "hoy" sea determinista en los tests.</summary>
public class RelojFijo : IReloj
{
    public DateOnly HoyFijo { get; set; } = new(2026, 10, 27);
    public DateOnly Hoy() => HoyFijo;
}

/// <summary>PostgreSQL real (Testcontainers) + la API en memoria (research R10).</summary>
public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public RelojFijo Reloj { get; } = new();
    public HttpClient Client { get; private set; } = null!;
    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = CrearFactory(new Dictionary<string, string?>());
        Client = _factory.CreateClient();
    }

    /// <summary>Crea otra instancia de la API sobre la misma base con configuración extra
    /// (p. ej. topes distintos).</summary>
    public WebApplicationFactory<Program> CrearFactory(IDictionary<string, string?> configuracion) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseSetting("ConnectionStrings:RoastingManager", ConnectionString);
            foreach (var (clave, valor) in configuracion)
            {
                b.UseSetting(clave, valor);
            }
            b.ConfigureServices(s =>
            {
                s.RemoveAll<IReloj>();
                s.AddSingleton<IReloj>(Reloj);
            });
        });

    public async Task LimpiarAsync()
    {
        await using var conexion = new NpgsqlConnection(ConnectionString);
        await conexion.OpenAsync();
        await using var comando = new NpgsqlCommand("""
            TRUNCATE venta_insumo, venta, batch, ingreso_stock, presentacion_insumo, presentacion,
                     insumo, cafe_verde, cliente, proveedor_insumo, proveedor_cafe
            RESTART IDENTITY CASCADE
            """, conexion);
        await comando.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Nombre)]
public class ColeccionApi : ICollectionFixture<ApiFixture>
{
    public const string Nombre = "api";
}

/// <summary>Base de los tests de integración: limpia la base antes de cada test.</summary>
[Collection(ColeccionApi.Nombre)]
public abstract class PruebaApi(ApiFixture fixture) : IAsyncLifetime
{
    protected ApiFixture Fixture { get; } = fixture;
    protected HttpClient Client => Fixture.Client;

    public async Task InitializeAsync()
    {
        Fixture.Reloj.HoyFijo = new DateOnly(2026, 10, 27);
        await Fixture.LimpiarAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
