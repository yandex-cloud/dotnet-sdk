
namespace Yandex.Cloud.Credentials
{
    public class IamTokenCredentialsProvider : ICredentialsProvider
    {
        private readonly string _iamToken;

        public IamTokenCredentialsProvider(string iamToken)
        {
            _iamToken = iamToken;
        }
        
        public string GetToken()
        {
            return _iamToken;
        }
    }
}