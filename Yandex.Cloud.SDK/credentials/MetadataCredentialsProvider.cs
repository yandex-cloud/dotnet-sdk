using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yandex.Cloud.Credentials
{
    public class MetadataCredentialsProvider : ICredentialsProvider
    {
        private readonly HttpClient _client = new HttpClient();

        public string GetToken()
        {
            var task = FetchToken();

            task.Wait();
            if (task.Exception != null)
            {
                throw task.Exception;
            }

            return task.Result;
        }

        private async Task<string> FetchToken(int retry = 0)
        {
            if (retry == 2)
            {
                throw new ApplicationException("failed to get token from metadata service after 3 retries");
            }

            var request = new HttpRequestMessage(HttpMethod.Get,
                "http://169.254.169.254/computeMetadata/v1/instance/service-accounts/default/token");
            request.Headers.Add("Metadata-Flavor", "Google");

            var response = await _client.SendAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return await FetchToken(++retry);
            }

            var data = await response.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<Token>(data);
            return token.access_token;
        }

        class Token
        {
            public string access_token { get; set; }

            //public string expires_in;
            //public string token_type;
        }
    }
}
