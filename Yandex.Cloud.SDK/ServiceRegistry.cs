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

        protected ChannelBase GetChannel(string serviceOverride = null)
        {
            return sdk.GetChannel(serviceOverride ?? service);
        }

        protected Sdk Sdk()
        {
            return sdk;
        }

        class CachingSdk : Sdk
        {
            private readonly Dictionary<string, ChannelBase> _channels = new Dictionary<string, ChannelBase>();

            public CachingSdk(ICredentialsProvider credentialsProvider) : base(credentialsProvider)
            {
            }

            public ChannelBase GetChannel(string service)
            {
                var endpointAddress = base.GetEndpointAddress(service);

                ChannelBase serviceChannel;
                lock (_channels)
                {
                    if (_channels.TryGetValue(service, out serviceChannel) &&
                        serviceChannel.Target == endpointAddress)
                    {
                        return serviceChannel;
                    }
                }

                serviceChannel = GetChannel(endpointAddress, base.GetCredentials());
                
                lock (_channels)
                {
                    _channels[service] = serviceChannel;

                    return serviceChannel;
                }
            }
        }
    }
}