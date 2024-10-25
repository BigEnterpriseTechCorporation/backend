using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend;

public static class TokenOptions
{
    public const string Issuer = "WebPracticeBackend"; // издатель токена
    public const string Audience = "WebPracticeFrontend"; // потребитель токена
    // ключ для шифрации
    private const string Key = "035d7b17b1814c1a7419621e2aa3e82d817b274c5043b081c8459741741e63d3bae15fda45e9a7852127474802e2b08bf3faa4df26932f1ec8d0456785547cf611b7054c310cab2014e919bc9537bd7b931ea4c83420320bad1a155c825c57da0c3cd3b0bbc4c6acc0cec02da891953c467b591501027620b5fbe5356164fc2f33f086374668e8617a9233b594a447d91f8efdba2bea5a73506e75af9d275ca3fe2db37f8fdb4f7cc56e0348276b461d7c13ad74b97d47eb8b28385b58d367b2df0a99963a0587ad29f9d687a0df987133916f092b6d4427d8f3db7e0766b63ebf9cf5f5fde29054ba8d76a66f2bc9c37693c5415f38d27b6a3bf8f027396b39";
    public const int Lifetime = 3 * 30 * 24 * 60; // время жизни токена - 3 месяца
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}