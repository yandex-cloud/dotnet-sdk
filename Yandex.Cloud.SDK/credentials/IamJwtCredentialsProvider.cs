using System;
using System.IdentityModel.Tokens.Jwt;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.IdentityModel.Tokens;
using Yandex.Cloud.Iam.V1;

namespace Yandex.Cloud.Credentials;

public class IamJwtCredentialsProvider : ICredentialsProvider
{
    private readonly RsaSecurityKey _key;
    
    private string _token;
    private DateTime _tokenExpiry;

    public IamJwtCredentialsProvider(RsaSecurityKey key, string serviceAccountId)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        ServiceAccountId = serviceAccountId;
    }
    
    public string ServiceAccountId { get; }

    public string GetToken()
    {
        if (string.IsNullOrEmpty(_token) || DateTime.UtcNow >= _tokenExpiry)
        {
            var response = TokenService().Create(new CreateIamTokenRequest
            {
                Jwt = CreateJwtToken()
            });
            
            _token = response.IamToken;
            _tokenExpiry = response.ExpiresAt.ToDateTime().AddMinutes(-5); // next refresh in 5 minutes before token expiration
        }

        return _token;
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
            Issuer = ServiceAccountId,
            Audience = "https://iam.api.cloud.yandex.net/iam/v1/tokens",
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(60),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.RsaSsaPssSha256)
        });
    }
}