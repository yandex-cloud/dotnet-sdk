using System;
using NUnit.Framework;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Resourcemanager.V1;

namespace Yandex.Cloud.SDK.Tests
{
    public class IntegrationTests
    {
        private Sdk _sdk;

        [SetUp]
        public void SetUp()
        {
            var token = Environment.GetEnvironmentVariable("YC_TOKEN");
            if (token == null)
            {
                Assert.Inconclusive("YC_TOKEN must be set to run integration tests");
            }

            _sdk = new Sdk(new OAuthCredentialsProvider(token));
        }

        [Test]
        public void TestListClouds()
        {
            var resp = _sdk.Services.Resourcemanager.CloudService.List(new ListCloudsRequest());
            Assert.NotZero(resp.Clouds.Count);
        }

        [Test]
        public void TestListFolders()
        {
            var cs = _sdk.Services.Resourcemanager.CloudService;
            var fs = _sdk.Services.Resourcemanager.FolderService;

            var clouds = cs.List(new ListCloudsRequest()).Clouds;
            Assert.NotZero(clouds.Count);

            var folders = fs.List(new ListFoldersRequest() {CloudId = clouds[0].Id}).Folders;
            Assert.NotZero(folders.Count);
        }

        [Test]
        public void TestSkuService_List()
        {
            var service = _sdk.Services.Billing.SkuService;

            var response = service.List(new Billing.V1.ListSkusRequest());
            Assert.NotZero(response.Skus.Count);
        }
    }
}