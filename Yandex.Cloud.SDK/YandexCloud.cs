using Grpc.Core;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Endpoint;
using Yandex.Cloud.Generated;

namespace Yandex.Cloud
{
    public class Sdk
    {
        public Sdk(ICredentialsProvider credentialsProvider)
        {
            _credentialsProvider = credentialsProvider;
            Services = new Services(this);
        }

        public Sdk() : this(new MetadataCredentialsProvider())
        {
        }

        public readonly Services Services;

        /////////

        private ApiEndpointService.ApiEndpointServiceClient EndpointService()
        {
            var channel = new Channel("api.cloud.yandex.net:443", _channelCredentials);
            return new ApiEndpointService.ApiEndpointServiceClient(channel);
        }

        private readonly ChannelCredentials _channelCredentials = new SslCredentials();
        private readonly ICredentialsProvider _credentialsProvider;

        public ChannelCredentials GetCredentials()
        {
            return ChannelCredentials.Create(
                _channelCredentials,
                CallCredentials.FromInterceptor(async (context, metadata) =>
                {
                    metadata.Add(
                        new Metadata.Entry(
                            "authorization",
                            $"Bearer {_credentialsProvider.GetToken()}"
                        )
                    );
                })
            );
        }

        public string GetEndpointAddress(string endpoint)
        {
            var req = new GetApiEndpointRequest {ApiEndpointId = endpoint};
            return EndpointService().Get(req).Address;
        }

        private Channel CreateChannelForEndpoint(string endpoint)
        {
            return new Channel(GetEndpointAddress(endpoint), GetCredentials());
        }
    }
}