using System;
using Grpc.Core;
using Yandex.Cloud.Endpoint;
using Yandex.Cloud.Iam.V1;

namespace Yandex.Cloud.Credentials
{
    public class OAuthCredentialsProvider : ICredentialsProvider
    {
        private readonly IamTokenService.IamTokenServiceClient _tokenService;
        private readonly string _oauthToken;
        private CreateIamTokenResponse _iamToken;

        public OAuthCredentialsProvider(string oauthToken)
        {
            _tokenService = TokenService();
            _oauthToken = oauthToken;
        }

        public OAuthCredentialsProvider(string oauthToken, IamTokenService.IamTokenServiceClient tokenService)
        {
            _tokenService = tokenService;
            _oauthToken = oauthToken;
        }

        private IamTokenService.IamTokenServiceClient TokenService()
        {
            var channel = new Channel("iam.api.cloud.yandex.net:443", new SslCredentials());
            return new IamTokenService.IamTokenServiceClient(channel);
        }

        public string GetToken()
        {
            var expiration = DateTimeOffset.Now.ToUnixTimeSeconds() + 300;
            if (_iamToken == null || _iamToken.ExpiresAt.Seconds <= expiration)
            {
                _iamToken = _tokenService.Create(new CreateIamTokenRequest()
                {
                    YandexPassportOauthToken = _oauthToken
                });
            }

            return _iamToken.IamToken;
        }
    }
}