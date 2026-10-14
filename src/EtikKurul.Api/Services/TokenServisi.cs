using System.Security.Claims;
using System.Text;
using EtikKurul.Api.Entities;
using EtikKurul.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EtikKurul.Api.Services;

/// <summary>
/// Giriş başarılı olunca kullanıcıya verilecek JWT'yi (token) üretir.
/// Token'ın içinde kullanıcının Id'si, e-postası, adı ve rolleri yazar.
/// Frontend her istekte bu token'ı "Authorization: Bearer ..." başlığıyla gönderir.
/// </summary>
public class TokenServisi(IOptions<JwtAyarlari> secenekler)
{
    private readonly JwtAyarlari _ayarlar = secenekler.Value;

    public (string Token, DateTime BitisZamani) Olustur(Kullanici kullanici, IEnumerable<string> roller)
    {
        var bitis = DateTime.UtcNow.AddMinutes(_ayarlar.SureDakika);

        var bilgiler = new List<Claim>
        {
            new("sub", kullanici.Id),
            new("email", kullanici.Email ?? string.Empty),
            new("name", $"{kullanici.Ad} {kullanici.Soyad}"),
        };
        bilgiler.AddRange(roller.Select(r => new Claim("role", r)));

        var anahtar = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_ayarlar.Anahtar));

        var tanim = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(bilgiler),
            Expires = bitis,
            Issuer = _ayarlar.Yayinci,
            Audience = _ayarlar.Hedef,
            SigningCredentials = new SigningCredentials(anahtar, SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(tanim);
        return (token, bitis);
    }
}
