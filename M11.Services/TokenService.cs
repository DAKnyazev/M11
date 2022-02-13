using M11.Common.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace M11.Services
{
    public class TokenService
    {
        private const string UrlFormat = "https://api.m11-neva.ru/onyma/system/api/json?function=open_session&user={0}&pass={1}&realm=M11.S1";

        public async Task<string> GetTokenAsync(string login, string password)
        {
            using var httpClient = new HttpClient();
            var url = string.Format(UrlFormat, login, password);
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<GetTokenResponse>();

            return result?.Return;
        }
    }
}
