using RoastingManager.Domain.Errores;
using RoastingManager.Domain.Reglas;

namespace RoastingManager.Domain.Calculos;

public record ResultadoBatch(decimal MermaPct, decimal CostoTostadoKg, decimal CostoTotal);

public static class CalculoBatch
{
    /// <summary>Merma (FR-011), costo del tostado en $/kg (FR-012) y costo total (FR-012a),
    /// cada uno redondeado con <see cref="Redondeo"/>.</summary>
    public static ResultadoBatch Calcular(long verdeG, long tostadoG, decimal precioKg)
    {
        ValidarCantidades(verdeG, tostadoG);

        var merma = Redondeo.Aplicar((verdeG - tostadoG) * 100m / verdeG);
        var costoTostado = Redondeo.Aplicar(precioKg * verdeG / tostadoG);
        var costoTotal = Redondeo.Aplicar(precioKg * verdeG / 1000m);
        return new ResultadoBatch(merma, costoTostado, costoTotal);
    }

    /// <summary>FR-008: verde y tostado mayores a cero, y tostado estrictamente menor al verde.</summary>
    public static void ValidarCantidades(long verdeG, long tostadoG)
    {
        Validacion.Ejecutar(
            () => ReglasComunes.ValidarPositivo("verdeKg", verdeG),
            () => ReglasComunes.ValidarPositivo("tostadoKg", tostadoG),
            () =>
            {
                if (verdeG > 0 && tostadoG >= verdeG)
                {
                    throw new ValidacionException("tostadoKg",
                        "El café tostado debe ser menor al café verde utilizado.");
                }
            });
    }
}
