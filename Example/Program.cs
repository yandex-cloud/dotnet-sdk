using System;
using System.Threading.Tasks;
using Yandex.Cloud.Resourcemanager.V1;
using Yandex.Cloud;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Storage.V1;

namespace Example
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var token = Environment.GetEnvironmentVariable("YC_TOKEN");
            if (token == null)
            {
                Console.WriteLine("YC_TOKEN must be set to run example");
                Environment.Exit(1);
            }
            
            var credProvider = new OAuthCredentialsProvider(token);
            var sdk = new Sdk(credProvider);

            var folder = UseResourceManager(sdk);
            if (folder == null)
            {
                Console.WriteLine("No folder found");
                return;
            }
            
            await UseStorage(sdk, folder);
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
}