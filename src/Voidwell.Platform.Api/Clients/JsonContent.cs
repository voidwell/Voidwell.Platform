using System.Text;
using System.Text.Json;

namespace Voidwell.Platform.Api.Clients;

public class JsonContent : StringContent
{
    public JsonContent(string content) : base(content, Encoding.UTF8, "application/json")
    {
    }

    public static JsonContent FromObject(object objectToSerialize)
    {
        var serializedValue = JsonSerializer.Serialize(objectToSerialize);
        return new JsonContent(serializedValue);
    }
}
