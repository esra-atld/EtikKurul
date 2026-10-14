using System.Security.Claims;

namespace EtikKurul.Api.Services;

public static class KullaniciUzantilari
{
    /// <summary>Token'daki "sub" bilgisinden giriş yapmış kullanıcının Id'si.</summary>
    public static string KullaniciId(this ClaimsPrincipal user) =>
        user.FindFirst("sub")?.Value
        ?? throw new InvalidOperationException("Token'da kullanıcı Id'si (sub) yok.");
}
