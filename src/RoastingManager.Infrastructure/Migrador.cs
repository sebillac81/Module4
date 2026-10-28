using DbUp;

namespace RoastingManager.Infrastructure;

/// <summary>Aplica los scripts SQL embebidos en Migrations/ (research R5). No se usa
/// <c>dotnet ef migrations</c>.</summary>
public static class Migrador
{
    public static void Ejecutar(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var resultado = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(Migrador).Assembly, s => s.Contains(".Migrations."))
            .WithTransactionPerScript()
            .LogToNowhere()
            .Build()
            .PerformUpgrade();

        if (!resultado.Successful)
        {
            throw new InvalidOperationException("No se pudo aplicar la migración del esquema.", resultado.Error);
        }
    }
}
