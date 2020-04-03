using System.Collections.Generic;

namespace tools
{
    public class GeneratorConfig
    {
        public static readonly Dictionary<string, EndpointConfig> Endpoints = new Dictionary<string, EndpointConfig>()
        {
            {"Services", new EndpointConfig("")},
            {"Services_Endpoint", new EndpointConfig("endpoint")},
            {"Services_Access", new EndpointConfig("access")},
            {"Services_Iam", new EndpointConfig("iam")},
            {"Services_Operation", new EndpointConfig("operation")},
            {"Services_Compute", new EndpointConfig("compute")},
            {"Services_Compute_Instancegroup", new EndpointConfig("compute")},
            {"Services_Serverless", new EndpointConfig("serverless")},
            {"Services_Serverless_Functions", new EndpointConfig("serverless-functions")},
            {"Services_Serverless_Triggers", new EndpointConfig("serverless-triggers")},
            {"Services_Resourcemanager", new EndpointConfig("resource-manager")},
            {"Services_Loadbalancer", new EndpointConfig("load-balancer")},
            {"Services_Ai", new EndpointConfig("ai")},
            {"Services_Ai_Translate", new EndpointConfig("ai-translate")},
            {"Services_Ai_Vision", new EndpointConfig("ai-vision")},
            {"Services_Ai_Stt", new EndpointConfig("ai-stt")},
            {"Services_Containerregistry", new EndpointConfig("container-registry")},
            {"Services_K8S", new EndpointConfig("managed-kubernetes")},
            {"Services_Vpc", new EndpointConfig("vpc")},
            {"Services_Iot", new EndpointConfig("iot")},
            {
                "Services_Iot_Devices", new EndpointConfig("iot-devices", new Dictionary<string, string>
                {
                    {"DeviceDataService", "iot-data"},
                    {"RegistryDataService", "iot-data"},
                })
            },
            {
                "Services_Kms", new EndpointConfig("kms", new Dictionary<string, string>()
                {
                    {"SymmetricCryptoService", "kms-crypt"},
                })
            }
        };
    }

    public class EndpointConfig
    {
        public readonly string Endpoint;
        public readonly Dictionary<string, string> ServiceEndpointOverrides;

        public EndpointConfig(string endpoint, Dictionary<string, string> serviceEndpointOverrides = null)
        {
            Endpoint = endpoint;
            ServiceEndpointOverrides = serviceEndpointOverrides ?? new Dictionary<string, string>();
        }
    }
}