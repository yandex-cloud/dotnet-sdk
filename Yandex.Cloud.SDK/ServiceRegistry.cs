using System;
using System.Collections.Generic;
using Grpc.Core;
using Yandex.Cloud.Credentials;

namespace Yandex.Cloud
{
    public abstract class ServiceRegistry
    {
        private readonly CachingSdk sdk;
        private readonly string service;

        protected ServiceRegistry(Sdk sdk, string service)
        {
            this.sdk = sdk is CachingSdk ? (CachingSdk)sdk : new CachingSdk(sdk.CredentialsProvider);
            this.service = service;
        }

        protected Channel GetChannel(string serviceOverride = null)
        {
            return sdk.GetChannel(serviceOverride ?? service);
        }

        protected Sdk Sdk()
        {
            return sdk;
        }

        class CachingSdk : Sdk
        {
            private readonly Dictionary<string, Channel> _channels = new Dictionary<string, Channel>();

            public CachingSdk(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
            {
            }

            public Channel GetChannel(string service)
            {
                var endpointAddress = base.GetEndpointAddress(service);

                lock (_channels)
                {
                    if (_channels.TryGetValue(service, out var serviceChannel) && serviceChannel.Target == endpointAddress)
                    {
                        return serviceChannel;
                    }

                    serviceChannel = new Channel(endpointAddress, base.GetCredentials());
                    _channels[service] = serviceChannel;

                    return serviceChannel;
                }
            }
        }
    }
}