using System;
using System.Collections.Generic;
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
            Assert.That(resp.Clouds, Is.Not.Empty);
        }

        [Test]
        public void TestListFolders()
        {
            var cs = _sdk.Services.Resourcemanager.CloudService;
            var fs = _sdk.Services.Resourcemanager.FolderService;

            var clouds = cs.List(new ListCloudsRequest()).Clouds;
            Assert.That(clouds, Is.Not.Empty);

            var folders = fs.List(new ListFoldersRequest() {CloudId = clouds[0].Id}).Folders;
            Assert.That(folders, Is.Not.Empty);
        }

        [Test]
        public void TestSkuService_List()
        {
            var service = _sdk.Services.Billing.SkuService;

            var response = service.List(new Billing.V1.ListSkusRequest());
            Assert.That(response.Skus, Is.Not.Empty);
        }

        [Test]
        public void TestFunctionContext()
        {
            var answer = 42;
            var foobar = "foobar";

            var values = new Dictionary<string, object>
            {
                { nameof(Yandex.Cloud.Functions.Context.MemoryLimitInMB), answer },
                { nameof(Yandex.Cloud.Functions.Context.FunctionId), foobar },
            };

            var context = new Yandex.Cloud.Functions.YcDictionaryFunctionContext(values, true);

            Assert.That(answer, Is.EqualTo(context.MemoryLimitInMB));
            Assert.That(foobar, Is.EqualTo(context.FunctionId));
        }
    }
}