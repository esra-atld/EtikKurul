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

    /// <summary>
    /// SADECE GELİŞTİRME İÇİN: appsettings.Development.json → "TestKullanicilari" listesindeki
    /// yetkili hesaplarını (arşiv kayıt, yetkili birim, 3 etik kurul üyesi) oluşturur.
    /// Böylece akışı denemek için her seferinde elle kayıt olup rol vermek gerekmez.
    /// Canlı ortamda bu ayar olmadığı için hiçbir şey yapmaz.
    /// </summary>
    public static async Task TestKullanicilariniYukleAsync(IServiceProvider servisler, IConfiguration ayarlar, ILogger logger)
    {
        var sifre = ayarlar["TestKullanicilari:Sifre"];
        var liste = ayarlar.GetSection("TestKullanicilari:Liste").GetChildren().ToList();
        if (string.IsNullOrWhiteSpace(sifre) || liste.Count == 0)
            return;

        var kullaniciYoneticisi = servisler.GetRequiredService<UserManager<Kullanici>>();

        foreach (var t in liste)
        {
            var email = t["Email"];
            var rol = t["Rol"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(rol) || !Roller.Tumu.Contains(rol))
                continue;
            if (await kullaniciYoneticisi.FindByEmailAsync(email) is not null)
                continue;

            // Etik kurul 3 üyeyle sınırlı
            if (rol == Roller.EtikKurul &&
                (await kullaniciYoneticisi.GetUsersInRoleAsync(Roller.EtikKurul)).Count >= 3)
            {
                logger.LogWarning("Etik kurulda zaten 3 üye var, {Email} eklenmedi.", email);
                continue;
            }

            var k = new Kullanici
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Ad = t["Ad"] ?? "Test",
                Soyad = t["Soyad"] ?? "Kullanıcı",
                CalistigiBirim = t["Birim"] ?? "Test",
                KurumsalEmail = email,
                PhoneNumber = t["Telefon"],
            };
            var sonuc = await kullaniciYoneticisi.CreateAsync(k, sifre);
            if (!sonuc.Succeeded)
            {
                logger.LogError("Test kullanıcısı oluşturulamadı ({Email}): {Hatalar}", email,
                    string.Join(", ", sonuc.Errors.Select(e => e.Description)));
                continue;
            }
            await kullaniciYoneticisi.AddToRoleAsync(k, rol);
            logger.LogInformation("Test kullanıcısı oluşturuldu: {Email} ({Rol})", email, rol);
        }
    }
}
