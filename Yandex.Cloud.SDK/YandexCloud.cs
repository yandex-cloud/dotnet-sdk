
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Endpoint;
using Yandex.Cloud.Generated;

namespace Yandex.Cloud
{
    public class Sdk
    {
        private readonly ChannelCredentials _channelCredentials = new SslCredentials();
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly ChannelBase _apiEndpointChannel;
        public readonly Services Services;


        public Sdk(ICredentialsProvider credentialsProvider)
        {
            _credentialsProvider = credentialsProvider;
            _apiEndpointChannel = GetChannel("api.cloud.yandex.net:443", _channelCredentials);
            Services = new Services(this);
        }

        public Sdk() : this(new MetadataCredentialsProvider())
        {
        }

        protected internal static ChannelBase GetChannel(string endpoint, ChannelCredentials credentials)
        {
            return GrpcChannel.ForAddress($"https://{endpoint}", new GrpcChannelOptions
            {
                Credentials = credentials
            });
        }

        public ChannelCredentials GetCredentials()
        {
            return ChannelCredentials.Create(
                _channelCredentials,
                CallCredentials.FromInterceptor((context, metadata) =>
                {
                    metadata.Add(
                        new Metadata.Entry(
                            "authorization",
                            $"Bearer {_credentialsProvider.GetToken()}"
                        )
                    );

                    return Task.CompletedTask;
                })
            );
        }

        public string GetEndpointAddress(string endpointId)
        {
            var client = new ApiEndpointService.ApiEndpointServiceClient(_apiEndpointChannel);

            var req = new GetApiEndpointRequest {ApiEndpointId = endpointId};
            var endpoint = client.Get(req);
            return endpoint.Address;
        }

        internal ICredentialsProvider CredentialsProvider => _credentialsProvider;
    }
}