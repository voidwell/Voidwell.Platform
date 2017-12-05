using System;
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

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
