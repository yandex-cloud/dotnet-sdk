using System;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Moq;
using NUnit.Framework;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Iam.V1;

namespace Yandex.Cloud.SDK.Tests.credentials
{
    public class OAuthCredentialsProviderTest
    {
        
        [Test]
        public void CreateToken_Ok()
        {
            var mockClient = new Mock<IamTokenService.IamTokenServiceClient>();
            mockClient
                .Setup(m => m.Create(
                    It.IsAny<CreateIamTokenRequest>(), null, null, CancellationToken.None))
                .Returns(new CreateIamTokenResponse
                {
                    IamToken = "t1.token",
                    ExpiresAt =
                        Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(300))
                });
            var provider = new OAuthCredentialsProvider(
                "OAuth",
                mockClient.Object);

            var token = provider.GetToken();

            Assert.AreEqual("t1.token", token);
            mockClient.Verify(m => m.Create(It.IsAny<CreateIamTokenRequest>(), null, null, CancellationToken.None),
                Times.Once);
        }

        [Test]
        public void CreateToken_Caching()
        {
            var mockClient = new Mock<IamTokenService.IamTokenServiceClient>();

            mockClient
                .SetupSequence(m => m.Create(
                    It.IsAny<CreateIamTokenRequest>(), null, null, CancellationToken.None))
                .Returns(
                    new CreateIamTokenResponse
                    {
                        IamToken = "t1.token",
                        ExpiresAt =
                            Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(1000))
                    })
                .Returns(
                    new CreateIamTokenResponse
                    {
                        IamToken = "t1.new_token",
                        ExpiresAt =
                            Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddHours(1))
                    });
            var provider = new OAuthCredentialsProvider(
                "OAuth",
                mockClient.Object);

            var token = provider.GetToken();

            Assert.AreEqual("t1.token", token);

            token = provider.GetToken();

            Assert.AreEqual("t1.token", token);
            mockClient.Verify(m => m.Create(It.IsAny<CreateIamTokenRequest>(), null, null, CancellationToken.None),
                Times.Once);
        }

        [Test]
        public void CreateToken_Renew()
        {
            var mockClient = new Mock<IamTokenService.IamTokenServiceClient>();

            mockClient
                .SetupSequence(m => m.Create(
                    It.IsAny<CreateIamTokenRequest>(), null, null, CancellationToken.None))
                .Returns(new CreateIamTokenResponse
                {
                    IamToken = "t1.token",
                    ExpiresAt =
                        Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(100))
                })
                .Returns(new CreateIamTokenResponse
                {
                    IamToken = "t1.new_token",
                    ExpiresAt =
                        Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddHours(1))
                });
            var provider = new OAuthCredentialsProvider(
                "OAuth",
                mockClient.Object);

            var token = provider.GetToken();

            Assert.AreEqual("t1.token", token);

            token = provider.GetToken();

            Assert.AreEqual("t1.new_token", token);

            mockClient.Verify(m => m.Create(It.IsAny<CreateIamTokenRequest>(), null, null, CancellationToken.None),
                Times.AtMost(2));
        }
    }
}