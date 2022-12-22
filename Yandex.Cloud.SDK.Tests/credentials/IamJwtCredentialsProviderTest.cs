using System;
using System.IO;
using System.Text;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Moq;
using NUnit.Framework;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Iam.V1;
using Org.BouncyCastle.OpenSsl;

namespace Yandex.Cloud.SDK.Tests.credentials
{
    public class IamJwtCredentialsProviderTest
    {
        private readonly string testKey = GeneratePrivateKey();

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
            var provider = new IamJwtCredentialsProvider(
                "serviceAccountId", "keyId", testKey,
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
            var provider = new IamJwtCredentialsProvider(
                "serviceAccountId", "keyId", testKey,
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
            var provider = new IamJwtCredentialsProvider(
                "serviceAccountId", "keyId", testKey,
                mockClient.Object);

            var token = provider.GetToken();

            Assert.AreEqual("t1.token", token);

            token = provider.GetToken();

            Assert.AreEqual("t1.new_token", token);

            mockClient.Verify(m => m.Create(It.IsAny<CreateIamTokenRequest>(), null, null, CancellationToken.None),
                Times.AtMost(2));
        }

        private static string GeneratePrivateKey()
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), 2048));

            AsymmetricCipherKeyPair keyPair = r.GenerateKeyPair();
            
            Pkcs8Generator pkcs8 = new Pkcs8Generator(keyPair.Private);
            var keyPem = new StringBuilder();
            StringWriter stringWriter = new StringWriter(keyPem);
            
            PemWriter pemWriter = new PemWriter(stringWriter);
            pemWriter.WriteObject(pkcs8);
            pemWriter.Writer.Flush();
            return keyPem.ToString();
        }
    }
}