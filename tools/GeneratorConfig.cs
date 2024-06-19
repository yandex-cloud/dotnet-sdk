using System.Collections.Generic;

namespace tools
{
    public class GeneratorConfig
    {
        public static readonly Dictionary<string, EndpointConfig> Endpoints = new Dictionary<string, EndpointConfig>()
        {
            {"Services", new EndpointConfig("")},
            {"Services_Access", new EndpointConfig("access")},
            {"Services_Ai", new EndpointConfig("ai")},
            {"Services_Ai_Stt", new EndpointConfig("ai-stt")},
            {"Services_Ai_Translate", new EndpointConfig("ai-translate")},
            {"Services_Ai_Vision", new EndpointConfig("ai-vision")},
            {"Services_Apploadbalancer", new EndpointConfig("application-load-balancer")},
            {"Services_Cdn", new EndpointConfig("cdn")},
            {
                "Services_Certificatemanager", new EndpointConfig("certificate-manager",
                    new Dictionary<string, string>()
                    {
                        {"CertificateContentService", "certificate-manager-data"}
                    })
            },
            {"Services_Compute", new EndpointConfig("compute")},
            {"Services_Compute_Instancegroup", new EndpointConfig("compute")},
            {"Services_Containerregistry", new EndpointConfig("container-registry")},
            {"Services_Dataproc", new EndpointConfig("dataproc")},
            {"Services_Dataproc_Manager", new EndpointConfig("dataproc-manager")},
            {"Services_Datasphere", new EndpointConfig("datasphere")},
            {"Services_Datatransfer", new EndpointConfig("datatransfer")},
            {"Services_Dns", new EndpointConfig("dns")},
            {"Services_Endpoint", new EndpointConfig("endpoint")},
            {"Services_Iam", new EndpointConfig("iam")},
            {"Services_Iam_Awscompatibility", new EndpointConfig("iam")},
            {"Services_Iot", new EndpointConfig("iot")},
            {"Services_K8S", new EndpointConfig("managed-kubernetes")},
            {"Services_Loadbalancer", new EndpointConfig("load-balancer")},
            {
                "Services_Logging", new EndpointConfig("logging", new Dictionary<string, string>
                {
                    {"LogReadingService", "log-reading"},
                    {"LogIngestionService", "log-ingestion"},
                })
            },
            {"Services_Marketplace", new EndpointConfig("marketplace")},
            {"Services_Marketplace_Licensemanager", new EndpointConfig("marketplace")},
            {"Services_Marketplace_Metering", new EndpointConfig("marketplace")},
            {"Services_Mdb", new EndpointConfig("mdb")},
            {"Services_Mdb_Clickhouse", new EndpointConfig("managed-clickhouse")},
            {"Services_Mdb_Elasticsearch", new EndpointConfig("managed-elasticsearch")},
            {"Services_Mdb_Greenplum", new EndpointConfig("managed-greenplum")},
            {"Services_Mdb_Kafka", new EndpointConfig("managed-kafka")},
            {"Services_Mdb_Mongodb", new EndpointConfig("managed-mongodb")},
            {"Services_Mdb_Mysql", new EndpointConfig("managed-mysql")},
            {"Services_Mdb_Postgresql", new EndpointConfig("managed-postgresql")},
            {"Services_Mdb_Redis", new EndpointConfig("managed-redis")},
            {"Services_Mdb_Sqlserver", new EndpointConfig("managed-sqlserver")},
            {"Services_Operation", new EndpointConfig("operation")},
            {"Services_Organizationmanager", new EndpointConfig("organization-manager")},
            {"Services_Organizationmanager_Saml", new EndpointConfig("organization-manager")},
            {"Services_Resourcemanager", new EndpointConfig("resource-manager")},
            {"Services_Serverless", new EndpointConfig("serverless")},
            {"Services_Serverless_Apigateway", new EndpointConfig("serverless-apigateway")},
            {"Services_Serverless_Containers", new EndpointConfig("serverless-containers")},
            {"Services_Serverless_Functions", new EndpointConfig("serverless-functions")},
            {"Services_Serverless_Mdbproxy", new EndpointConfig("mdbproxy")},
            {"Services_Serverless_Triggers", new EndpointConfig("serverless-triggers")},
            {"Services_Storage", new EndpointConfig("storage-api")},
            {"Services_Vpc", new EndpointConfig("vpc")},
            {"Services_Ydb", new EndpointConfig("ydb")},
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
                    {"SymmetricCryptoService", "kms-crypto"},
                })
            },
            {
                "Services_Lockbox", new EndpointConfig("lockbox", new Dictionary<string, string>()
                {
                    {"PayloadService", "lockbox-payload"},
                })
            },
            {"Services_Billing", new EndpointConfig("billing")}
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