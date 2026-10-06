using System.Text.Json;

namespace Voidwell.Platform.Clients;

public static class HttpContentExtensions
{
    public static async Task<T?> ReadAsObjectAsync<T>(this HttpContent content)
    {
        var serializedString = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(serializedString, JsonSerializerOptions.Web);
    }
}
