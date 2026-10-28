using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RoastingManager.Api.Tests.Infra;

public static class Http
{
    public static async Task<JsonElement> PostOk(this HttpClient client, string ruta, object body)
    {
        var respuesta = await client.PostAsJsonAsync(ruta, body);
        var json = await respuesta.Content.ReadAsStringAsync();
        Assert.True(respuesta.StatusCode == HttpStatusCode.Created, $"POST {ruta} → {(int)respuesta.StatusCode}: {json}");
        return JsonDocument.Parse(json).RootElement;
    }

    public static async Task<(HttpStatusCode Status, JsonElement Json)> Post(this HttpClient client, string ruta, object body)
    {
        var respuesta = await client.PostAsJsonAsync(ruta, body);
        var json = await respuesta.Content.ReadAsStringAsync();
        return (respuesta.StatusCode, json.Length == 0 ? default : JsonDocument.Parse(json).RootElement);
    }

    public static async Task<JsonElement> GetOk(this HttpClient client, string ruta)
    {
        var respuesta = await client.GetAsync(ruta);
        var json = await respuesta.Content.ReadAsStringAsync();
        Assert.True(respuesta.StatusCode == HttpStatusCode.OK, $"GET {ruta} → {(int)respuesta.StatusCode}: {json}");
        return JsonDocument.Parse(json).RootElement;
    }

    public static decimal Dec(this JsonElement e, string propiedad) => e.GetProperty(propiedad).GetDecimal();
    public static long Id(this JsonElement e) => e.GetProperty("id").GetInt64();
}
