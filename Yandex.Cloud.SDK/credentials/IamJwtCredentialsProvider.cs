using System;
using System.IO;
using System.Security.Cryptography;
using Grpc.Core;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Yandex.Cloud.Iam.V1;

namespace Yandex.Cloud.Credentials
{
    public class IamJwtCredentialsProvider : ICredentialsProvider
    {
        private readonly string _keyId;
        private readonly string _pemCertificate;
        private readonly string _serviceAccountId;
        private readonly IamTokenService.IamTokenServiceClient _tokenService;
        private CreateIamTokenResponse _iamToken;

        public IamJwtCredentialsProvider(string serviceAccountId, string keyId, string pemCertificate)
        {
            _serviceAccountId = serviceAccountId;
            _keyId = keyId;
            _pemCertificate = pemCertificate;
            _tokenService = TokenService();
        }

        public IamJwtCredentialsProvider(string serviceAccountId, string keyId, string pemCertificate,
            IamTokenService.IamTokenServiceClient tokenService)
        {
            _serviceAccountId = serviceAccountId;
            _keyId = keyId;
            _pemCertificate = pemCertificate;
            _tokenService = tokenService;
        }

        public string GetToken()
        {
            var expiration = DateTimeOffset.Now.ToUnixTimeSeconds() + 300;
            if (_iamToken == null || _iamToken.ExpiresAt.Seconds <= expiration)
                _iamToken = _tokenService.Create(new CreateIamTokenRequest
                {
                    Jwt = GetJwtToken()
                });

            return _iamToken.IamToken;
        }

        private IamTokenService.IamTokenServiceClient TokenService()
        {
            var channel = new Channel("iam.api.cloud.yandex.net:443", new SslCredentials());
            return new IamTokenService.IamTokenServiceClient(channel);
        }

        private static RSAParameters ToRsaParameters(RsaPrivateCrtKeyParameters privKey)
        {
            var rp = new RSAParameters
            {
                Modulus = privKey.Modulus.ToByteArrayUnsigned(),
                Exponent = privKey.PublicExponent.ToByteArrayUnsigned(),
                D = privKey.Exponent.ToByteArrayUnsigned(),
                P = privKey.P.ToByteArrayUnsigned(),
                Q = privKey.Q.ToByteArrayUnsigned(),
                DP = privKey.DP.ToByteArrayUnsigned(),
                DQ = privKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privKey.QInv.ToByteArrayUnsigned()
            };
            return rp;
        }

        private string GetJwtToken()
        {
            var handler = new JsonWebTokenHandler();
            var now = DateTime.UtcNow;
            RsaPrivateCrtKeyParameters privateKeyParams = null;
            using (var pemStream = new StringReader(_pemCertificate))
            {
                privateKeyParams = new PemReader(pemStream).ReadObject() as RsaPrivateCrtKeyParameters;
            }

            var key = new RsaSecurityKey(ToRsaParameters(privateKeyParams))
            {
                KeyId = _keyId
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _serviceAccountId,
                Audience = "https://iam.api.cloud.yandex.net/iam/v1/tokens",
                IssuedAt = now,
                NotBefore = now,
                Expires = now.AddMinutes(60),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSsaPssSha256)
            };

            return handler.CreateToken(descriptor);
        }
    }
}