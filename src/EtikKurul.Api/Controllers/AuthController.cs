using EtikKurul.Api.Constants;
using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;
using EtikKurul.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtikKurul.Api.Controllers;

/// <summary>Kayıt olma, giriş yapma ve giriş yapmış kullanıcının bilgileri.</summary>
[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<Kullanici> kullaniciYoneticisi,
    TokenServisi tokenServisi,
    IConfiguration ayarlar) : ControllerBase
{
    /// <summary>Yeni kullanıcı kaydı. Kayıt olan herkes "Basvuran" rolüyle başlar.</summary>
    [HttpPost("kayit")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Kayit(KayitIstegi istek)
    {
        // Kurumsal e-posta üniversite uzantılı olmalı (ör. @selcuk.edu.tr, @ogr.selcuk.edu.tr)
        var uzanti = ayarlar["KurumsalEmailUzantisi"];
        if (!string.IsNullOrWhiteSpace(uzanti) &&
            !istek.KurumsalEmail.Trim().EndsWith(uzanti, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(istek.KurumsalEmail),
                $"Kurumsal e-posta '{uzanti}' ile bitmelidir.");
            return ValidationProblem(ModelState);
        }

        var kurumsalEmail = istek.KurumsalEmail.Trim();
        if (await kullaniciYoneticisi.Users.AnyAsync(k => k.KurumsalEmail == kurumsalEmail))
        {
            ModelState.AddModelError(nameof(istek.KurumsalEmail), "Bu kurumsal e-posta zaten kayıtlı.");
            return ValidationProblem(ModelState);
        }

        var kullanici = new Kullanici
        {
            UserName = istek.Email.Trim(),
            Email = istek.Email.Trim(),
            Ad = istek.Ad.Trim(),
            Soyad = istek.Soyad.Trim(),
            CalistigiBirim = istek.CalistigiBirim.Trim(),
            KurumsalEmail = kurumsalEmail,
            PhoneNumber = istek.Telefon.Trim(),
            Adres = istek.Adres.Trim(),
        };

        var sonuc = await kullaniciYoneticisi.CreateAsync(kullanici, istek.Sifre);
        if (!sonuc.Succeeded)
        {
            foreach (var hata in sonuc.Errors)
                ModelState.AddModelError(hata.Code, hata.Description);
            return ValidationProblem(ModelState);
        }

        await kullaniciYoneticisi.AddToRoleAsync(kullanici, Roller.Basvuran);

        return StatusCode(StatusCodes.Status201Created, new { mesaj = "Kayıt başarılı. Giriş yapabilirsiniz." });
    }

    /// <summary>E-posta ve şifre ile giriş. Başarılıysa token döner.</summary>
    [HttpPost("giris")]
    [ProducesResponseType<GirisCevabi>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Giris(GirisIstegi istek)
    {
        var kullanici = await kullaniciYoneticisi.FindByEmailAsync(istek.Email.Trim());

        // Güvenlik: "böyle bir kullanıcı yok" ile "şifre yanlış" arasında fark söylemiyoruz.
        if (kullanici is null)
            return Unauthorized(new { mesaj = "E-posta veya şifre hatalı." });

        if (!kullanici.AktifMi)
            return Unauthorized(new { mesaj = "Hesabınız pasif durumda. Yöneticiyle iletişime geçin." });

        if (await kullaniciYoneticisi.IsLockedOutAsync(kullanici))
            return Unauthorized(new { mesaj = "Çok fazla hatalı deneme. Lütfen birkaç dakika sonra tekrar deneyin." });

        if (!await kullaniciYoneticisi.CheckPasswordAsync(kullanici, istek.Sifre))
        {
            await kullaniciYoneticisi.AccessFailedAsync(kullanici); // 5 hatada hesap geçici kilitlenir
            return Unauthorized(new { mesaj = "E-posta veya şifre hatalı." });
        }

        await kullaniciYoneticisi.ResetAccessFailedCountAsync(kullanici);

        var roller = await kullaniciYoneticisi.GetRolesAsync(kullanici);
        var (token, bitis) = tokenServisi.Olustur(kullanici, roller);

        return Ok(new GirisCevabi(token, bitis, KullaniciEslestirici.DtoyaCevir(kullanici, roller)));
    }

    /// <summary>Giriş yapmış kullanıcının kendi bilgileri. Token gerektirir.</summary>
    [Authorize]
    [HttpGet("ben")]
    [ProducesResponseType<KullaniciDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Ben()
    {
        var id = User.FindFirst("sub")?.Value;
        var kullanici = id is null ? null : await kullaniciYoneticisi.FindByIdAsync(id);
        if (kullanici is null)
            return Unauthorized();

        var roller = await kullaniciYoneticisi.GetRolesAsync(kullanici);
        return Ok(KullaniciEslestirici.DtoyaCevir(kullanici, roller));
    }
}
