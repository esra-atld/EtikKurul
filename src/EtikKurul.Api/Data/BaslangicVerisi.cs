using EtikKurul.Api.Constants;
using EtikKurul.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace EtikKurul.Api.Data;

/// <summary>
/// Uygulama her açıldığında çalışır:
/// 1) 5 rol veritabanında yoksa oluşturur.
/// 2) appsettings'teki "BaslangicAdmin" bilgileriyle bir admin kullanıcısı yoksa oluşturur.
/// Zaten varsa hiçbir şey yapmaz, yani tekrar tekrar çalışması sorun değil.
/// </summary>
public static class BaslangicVerisi
{
    public static async Task YukleAsync(IServiceProvider servisler, IConfiguration ayarlar, ILogger logger)
    {
        var rolYoneticisi = servisler.GetRequiredService<RoleManager<IdentityRole>>();
        var kullaniciYoneticisi = servisler.GetRequiredService<UserManager<Kullanici>>();

        foreach (var rol in Roller.Tumu)
        {
            if (!await rolYoneticisi.RoleExistsAsync(rol))
            {
                await rolYoneticisi.CreateAsync(new IdentityRole(rol));
                logger.LogInformation("Rol oluşturuldu: {Rol}", rol);
            }
        }

        var adminEmail = ayarlar["BaslangicAdmin:Email"];
        var adminSifre = ayarlar["BaslangicAdmin:Sifre"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminSifre))
        {
            logger.LogWarning("BaslangicAdmin ayarı yok, admin kullanıcısı oluşturulmadı.");
            return;
        }

        if (await kullaniciYoneticisi.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new Kullanici
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Ad = "Sistem",
            Soyad = "Yöneticisi",
            CalistigiBirim = "Bilgi İşlem",
            KurumsalEmail = adminEmail,
        };

        var sonuc = await kullaniciYoneticisi.CreateAsync(admin, adminSifre);
        if (!sonuc.Succeeded)
        {
            logger.LogError("Admin oluşturulamadı: {Hatalar}",
                string.Join(", ", sonuc.Errors.Select(e => e.Description)));
            return;
        }

        await kullaniciYoneticisi.AddToRoleAsync(admin, Roller.Admin);
        logger.LogInformation("Admin kullanıcısı oluşturuldu: {Email}", adminEmail);
    }
}
