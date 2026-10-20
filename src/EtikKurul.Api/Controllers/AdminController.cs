using EtikKurul.Api.Constants;
using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;
using EtikKurul.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtikKurul.Api.Controllers;

/// <summary>
/// Yetkili kişilerin tanımlandığı yer: kullanıcıları listeleme, rol atama, pasif yapma.
/// Sadece Admin rolündekiler erişebilir.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roller.Admin)]
public class AdminController(UserManager<Kullanici> kullaniciYoneticisi) : ControllerBase
{
    /// <summary>Sistemdeki rollerin listesi.</summary>
    [HttpGet("roller")]
    public IActionResult RolleriGetir() => Ok(Roller.Tumu);

    /// <summary>Tüm kullanıcılar ve rolleri. İsteğe bağlı olarak ad, soyad veya e-postaya göre arama.</summary>
    [HttpGet("kullanicilar")]
    public async Task<ActionResult<List<KullaniciDto>>> KullanicilariGetir([FromQuery] string? arama)
    {
        var sorgu = kullaniciYoneticisi.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var a = arama.Trim();
            sorgu = sorgu.Where(k =>
                k.Ad.Contains(a) || k.Soyad.Contains(a) ||
                (k.Email != null && k.Email.Contains(a)) || k.KurumsalEmail.Contains(a));
        }

        var kullanicilar = await sorgu.OrderBy(k => k.Ad).ThenBy(k => k.Soyad).ToListAsync();

        var liste = new List<KullaniciDto>();
        foreach (var k in kullanicilar)
            liste.Add(KullaniciEslestirici.DtoyaCevir(k, await kullaniciYoneticisi.GetRolesAsync(k)));

        return Ok(liste);
    }

    /// <summary>Kullanıcının rollerini verilen listeyle değiştirir (ör. ["EtikKurul"]).</summary>
    [HttpPut("kullanicilar/{id}/roller")]
    public async Task<IActionResult> RolleriGuncelle(string id, RolGuncellemeIstegi istek)
    {
        var gecersiz = istek.Roller.Except(Roller.Tumu).ToList();
        if (gecersiz.Count > 0)
            return BadRequest(new { mesaj = $"Geçersiz rol: {string.Join(", ", gecersiz)}" });

        var kullanici = await kullaniciYoneticisi.FindByIdAsync(id);
        if (kullanici is null)
            return NotFound(new { mesaj = "Kullanıcı bulunamadı." });

        // Admin kendi admin yetkisini yanlışlıkla kaldırıp sistemi sahipsiz bırakmasın.
        var benimId = User.FindFirst("sub")?.Value;
        if (id == benimId && !istek.Roller.Contains(Roller.Admin))
            return BadRequest(new { mesaj = "Kendi Admin rolünüzü kaldıramazsınız." });

        var mevcut = await kullaniciYoneticisi.GetRolesAsync(kullanici);

        // Etik kurulun 3 üyesi olur; 4. kişiye bu rol verilemez
        if (istek.Roller.Contains(Roller.EtikKurul) && !mevcut.Contains(Roller.EtikKurul))
        {
            var uyeSayisi = (await kullaniciYoneticisi.GetUsersInRoleAsync(Roller.EtikKurul)).Count;
            if (uyeSayisi >= YonetimController.EtikKurulUyeSayisi)
                return BadRequest(new
                {
                    mesaj = $"Etik kurulun {YonetimController.EtikKurulUyeSayisi} üyesi zaten tanımlı. " +
                            "Yeni üye eklemek için önce bir üyenin etik kurul yetkisini kaldırın.",
                });
        }
        var cikarilacak = mevcut.Except(istek.Roller).ToList();
        var eklenecek = istek.Roller.Distinct().Except(mevcut).ToList();

        if (cikarilacak.Count > 0)
            await kullaniciYoneticisi.RemoveFromRolesAsync(kullanici, cikarilacak);
        if (eklenecek.Count > 0)
            await kullaniciYoneticisi.AddToRolesAsync(kullanici, eklenecek);

        var yeniRoller = await kullaniciYoneticisi.GetRolesAsync(kullanici);
        return Ok(KullaniciEslestirici.DtoyaCevir(kullanici, yeniRoller));
    }

    /// <summary>Kullanıcıyı aktif veya pasif yapar. Pasif kullanıcı giriş yapamaz.</summary>
    [HttpPut("kullanicilar/{id}/aktiflik")]
    public async Task<IActionResult> AktiflikGuncelle(string id, AktiflikIstegi istek)
    {
        var kullanici = await kullaniciYoneticisi.FindByIdAsync(id);
        if (kullanici is null)
            return NotFound(new { mesaj = "Kullanıcı bulunamadı." });

        if (id == User.FindFirst("sub")?.Value && !istek.AktifMi)
            return BadRequest(new { mesaj = "Kendinizi pasif yapamazsınız." });

        kullanici.AktifMi = istek.AktifMi;
        await kullaniciYoneticisi.UpdateAsync(kullanici);

        return Ok(KullaniciEslestirici.DtoyaCevir(kullanici, await kullaniciYoneticisi.GetRolesAsync(kullanici)));
    }
}
