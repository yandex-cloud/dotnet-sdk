using Grpc.Core;

namespace Yandex.Cloud
{
    public abstract class ServiceRegistry
    {
        private readonly Sdk sdk;
        private readonly string endpoint;

        protected ServiceRegistry(Sdk sdk, string endpoint)
        {
            this.sdk = sdk;
            this.endpoint = endpoint;
        }

        protected Channel GetChannel(string endpointOverride = null)
        {
            return new Channel(sdk.GetEndpointAddress(endpointOverride ?? endpoint), sdk.GetCredentials());
        }

        protected Sdk Sdk()
        {
            return sdk;
        }
    }
}