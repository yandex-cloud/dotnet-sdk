using System;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Moq;
using NUnit.Framework;
using Yandex.Cloud.Credentials;
using Yandex.Cloud.Iam.V1;

namespace Yandex.Cloud.SDK.Tests.credentials
{
    public class IamJwtCredentialsProviderTest
    {
        private readonly string testKey =
            "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCghQKOzS6OBVjq\nLS63GpIIHEc11deimiPHltRLmTGT/bgF/ymflBbCiR4L5zxx5cYy1J2Ec5IW3CNR\nFMlMaD7/Zyt5lpTEElT9HeZc5PaJAnTHuedmomuu8zIVQcNZfItB2+iWsf7QxZcA\nQdOqO4F1XdQW0jAJLE6ROH/ydmlHnaVZAcW1RZY9ePyvePQnS8txIqADM6J9rMNQ\nDUUNThTgeRpczcBSPrtSQVPMKTq5dK2nKNMRc+Lc7Kupljxp4jKJuIvMYh51mcQW\nbrWXugAlihPT5ZToCp7IX/cHTidbRSFGkLTud3pZsabVnZ7Hy3i9HmECv/9n1JaT\nYIePFMcDAgMBAAECggEBAIqFvktVrdNJsg2EqEfDWjo9jQZcYBYmRrI660HCFhLM\nZT5nkJfxyCJhCdjj2DnCPGQpLzXaNjwcBS43be/OFm95rP7kf8UkvMsiPmX0AP0D\nbZveRl8/0wXLQXEuq1JSNCrFh9ZkDK5FlaXXKL5DU9jg++IlyOZhofuYcnHe1cPC\nXGwY1vmlv1lGBGVRlj4hNjsv0QK6fcbNT2TFTEM0NwH1fq1Q6O/NkNcbsBAuMZo2\n8ktLh70BjzpLgFXjV+pDkLZXFOTsCBuXuVr+4ieOLNvrCoumYmeB3uu+SjUOeSDB\nef6F2BhEFn4uL+T5lEwT7gQKTDZfADvYqTe1NW0eSYECgYEA3XVe370gphneREHg\n2qizpSc+kzhatbLl5kq+JFXoaZFhIENmv4mXvUlrflH53UMvw7SaFfxeTw1LIodu\nf2X+oytYnfYRxy3uvTY1OygwhYTghGKnZSaMb4jllUE/R8VBj0w3q5iCUCsiZYQZ\nOfnlgNn9XWNP8tjgfOYUck4AG9MCgYEAuY5n6dCLFljZCJYNTSbthHlWVXVH6YJS\nKRxRkUK9BTaYEtr+NrfbVW3KqpEY0KsU7khjvck7i2R3t5ZFJwAI4Yr9HDl+eGEb\nkbMxprrbNRddilenrjxEogOmiJPiKO/KIC6aMBvyBRvVwBT7eIhDBf3soeuKYaS7\nHbaAjIaSmhECgYBAUmXTAGCqPdsz4hqIB/Xdoy9mnTiji6mAoPbnINiXSPV35HvE\naBddkCSblP3IyUnnQt56Qkm2FjtbdRCgCiTSibV8c8soGew8orEFVJ/7N+PqL6lF\nFNQ3VQCxwDi8BMwM7etBeNNoq46bA6o8D5wcN7SCWmUPOPTC8iTAKm2XpwKBgDvT\naDmAnXeFCeS0zHEVnfraTDWdbKQi+m56zOmyxNLLeViK9u+Zc8Qlc/Rud7u/jS4O\ndOoZ2XLl35t3fbBHWJWvhE/3UYlqdcMSLW3+TPLfJ6+U92E72T0LRERAuehM97RM\nkjX0rKFtCgu2OCXyePmTlxnVoXY98H5x+xxTvyyhAoGAdXokZTuDkqNboAf4kX5a\nBtwHpXYOBncpy3wuTteKqkp536kbFUatQNlDF7WeLpsgIjfEfxt4WfMNW235WUZm\nfBG/N2t29ibIP5ZslKdOzXkvqc7eW+ugAPbbbPjbQrf0iNJERNtd1nYS1CmCl7tX\n3gFX7DDJzyffsOVQ8m1XHVE=\n-----END PRIVATE KEY-----\n";


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
    }
}