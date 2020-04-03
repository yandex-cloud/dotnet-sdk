using System;
using Yandex.Cloud.Endpoint;
using Yandex.Cloud.Resourcemanager.V1;
using Yandex.Cloud;
using Yandex.Cloud.Credentials;

namespace Example
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var sdk = new Sdk();
            var response = sdk.Services.Resourcemanager.CloudService.List(new ListCloudsRequest());

            foreach (var c in response.Clouds)
            {
                Console.Out.WriteLine($"* {c.Name} ({c.Id})");
            }
        }
    }
}