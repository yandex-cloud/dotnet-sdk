using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Yandex.Cloud.Resourcemanager.V1;
using Yandex.Cloud;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Iam.V1;
using Yandex.Cloud.Lockbox.V1;
using Yandex.Cloud.Storage.V1;

namespace Example
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await WithOAuthCredentials(async credProvider =>
                {
                    var sdk = new Sdk(credProvider);

                    var folder = UseResourceManager(sdk);
                    if (folder == null)
                    {
                        Console.WriteLine("No folder found");
                        return;
                    }

                    await UseStorage(sdk, folder);
                });

            await WithIamJwtCredentials(async credProvider =>
            {
                var sdk = new Sdk(credProvider);
                var serviceAccount = await sdk.Services.Iam.ServiceAccountService.GetAsync(new GetServiceAccountRequest
                {
                    ServiceAccountId = credProvider.ServiceAccountId
                });
                
                var listResponse = await sdk.Services.Lockbox.SecretService.ListAsync(new ListSecretsRequest
                {
                    FolderId = serviceAccount.FolderId
                });

                if (listResponse.Secrets.Count == 0)
                {
                    Console.WriteLine("No secrets found");
                }

                foreach (var secret in listResponse.Secrets)
                {
                    Console.WriteLine(secret.Id);
                }
            });
        }
        
        private static async Task WithOAuthCredentials(Func<ICredentialsProvider, Task> action)
        {
            var token = Environment.GetEnvironmentVariable("YC_TOKEN");
            if (token == null)
            {
                Console.WriteLine("YC_TOKEN must be set to run example");
                return;
            }
            
            var credProvider = new OAuthCredentialsProvider(token);
            await action(credProvider);
        }

        private static async Task WithIamJwtCredentials(Func<IamJwtCredentialsProvider, Task> action)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length < 3) return;

            var cmd = args[1];
            if (string.Compare(cmd, "json", StringComparison.OrdinalIgnoreCase) != 0)
            {
                Console.WriteLine("No json file specified");
                return;
            }
            var path = args[2];
            var json = await File.ReadAllTextAsync(path);
            var container = JsonSerializer.Deserialize<JsonContainer>(json);
        
            var rsa = RSA.Create();
            rsa.ImportFromPem(container.PrivateKey);
                
            var key = new RsaSecurityKey(rsa)
            {
                KeyId = container.Id
            };
            
            var credProvider = new IamJwtCredentialsProvider(key, container.ServiceAccountId);
            await action(credProvider);
        }

        private static Folder UseResourceManager(Sdk sdk)
        {
            var cloudsResponse = sdk.Services.Resourcemanager.CloudService.List(new ListCloudsRequest());

            var cloudId = string.Empty;
            Console.WriteLine("Clouds:");
            foreach (var c in cloudsResponse.Clouds)
            {
                cloudId = c.Id;
                Console.Out.WriteLine($"* {c.Name} ({c.Id})");
            }
            
            var folderResponse = sdk.Services.Resourcemanager.FolderService.List(new ListFoldersRequest { CloudId = cloudId });            
            foreach (var folder in folderResponse.Folders)
            {
                return folder;
            }

            return null;
        }

        private static async Task UseStorage(Sdk sdk, Folder folder)
        {
            var bucketName = $"sdk-example-bucket-{DateTime.Now:yyMMddHHmmss}";
            var operation = await sdk.Services.Storage.BucketService
                .CreateAsync(new CreateBucketRequest { FolderId = folder.Id, Name = bucketName }).ResponseAsync;

            await sdk.WaitForCompletionAsync(operation);
            
            var listResponse = await sdk.Services.Storage.BucketService.ListAsync(new ListBucketsRequest{FolderId = folder.Id}).ResponseAsync;
            
            Console.WriteLine("Buckets:");
            foreach (var bucket in listResponse.Buckets)
            {
                Console.WriteLine(bucket.Name);
            }
            
            await sdk.Services.Storage.BucketService.DeleteAsync(new DeleteBucketRequest { Name = bucketName }).WaitForCompletionAsync(sdk);
        }
    }
    
    class JsonContainer
    {
        [JsonRequired]
        [JsonPropertyName("id")]
        public string Id { get; init; }
        
        [JsonRequired]
        [JsonPropertyName("service_account_id")]
        public string ServiceAccountId {get; init;}
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; init; }
        
        [JsonPropertyName("key_algorithm")]
        public string KeyAlgorithm { get; init; }
        
        [JsonPropertyName("public_key")]
        public string PublicKey { get; init; }
        
        [JsonRequired]
        [JsonPropertyName("private_key")]
        public string PrivateKey { get; init; }
    }
}