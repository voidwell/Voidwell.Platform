using Voidwell.Platform.Api.Models;

namespace Voidwell.Platform.Api.Clients;

public class UserManagementClient : IUserManagementClient
{
    private readonly HttpClient _httpClient;

    public UserManagementClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DisplayName?> GetDisplayNameAsync(Guid userId)
    {
        var response = await _httpClient.GetAsync($"user/{userId}/name");
        return await response.GetContentAsync<DisplayName>();
    }

    public async Task<IEnumerable<DisplayName>?> GetDisplayNamesAsync(IEnumerable<Guid> userIds)
    {
        var batchRequest = new BatchUserRequest(userIds);
        using var content = JsonContent.FromObject(batchRequest);
        var response = await _httpClient.PostAsync("user/names", content);
        return await response.GetContentAsync<IEnumerable<DisplayName>>();
    }
}
