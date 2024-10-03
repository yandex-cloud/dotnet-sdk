using System;
using System.IdentityModel.Tokens.Jwt;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.IdentityModel.Tokens;
using Yandex.Cloud.Iam.V1;

namespace Yandex.Cloud.Credentials;

public class IamJwtCredentialsProvider : ICredentialsProvider
{
    private RsaSecurityKey _key;
    private string _serviceAccountId;

    public IamJwtCredentialsProvider(RsaSecurityKey key, string serviceAccountId)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _serviceAccountId = serviceAccountId;
    }

    public string GetToken()
    {
        // TODO: reuse non-expired token
        var response = TokenService().Create(new CreateIamTokenRequest
        {
            Jwt = CreateJwtToken()
        });

        return response.IamToken;
    }
    
    private IamTokenService.IamTokenServiceClient TokenService()
    {
        var channel = GrpcChannel.ForAddress("https://iam.api.cloud.yandex.net:443", new GrpcChannelOptions
        {
            Credentials = new SslCredentials()
        });
        return new IamTokenService.IamTokenServiceClient(channel);
    }

    private string CreateJwtToken()
    {
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        return handler.CreateEncodedJwt(new SecurityTokenDescriptor
        {
            Issuer = _serviceAccountId,
            Audience = "https://iam.api.cloud.yandex.net/iam/v1/tokens",
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(60),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSsaPssSha256)
        });
    }
}