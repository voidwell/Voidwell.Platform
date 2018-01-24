using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Voidwell.Internal.Models;

namespace Voidwell.Internal.Clients
{
    public class UserManagementClient : IUserManagementClient, IDisposable
    {
        private readonly HttpClient _httpClient;

        public UserManagementClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://voidwellusermanagement:5000");
        }

        public async Task<DisplayName> GetDisplayName(Guid userId)
        {
            var response = await _httpClient.GetAsync($"user/{userId}/name");
            return await response.GetContentAsync<DisplayName>();
        }

        public async Task<IEnumerable<DisplayName>> GetDisplayNames(IEnumerable<Guid> userIds)
        {
            var batchRequest = new BatchUserRequest(userIds);
            var content = JsonContent.FromObject(batchRequest);
            var response = await _httpClient.PostAsync("user/names", content);
            return await response.GetContentAsync<IEnumerable<DisplayName>>();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
