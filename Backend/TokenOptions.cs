using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend;

public static class TokenOptions
{
    public const string Issuer = "WebPracticeBackend"; // издатель токена
    public const string Audience = "WebPracticeFrontend"; // потребитель токена
    // ключ для шифрации
    private const string Key = "eVUlkE~MV1M*dLOR27aP3~btkt6SPwLW}?vj8iZl8T5tzgO5%#l|OhO8tWq#%fpx";
    public const int Lifetime = 3 * 30 * 24 * 60; // время жизни токена - 3 месяца
    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
    }
}