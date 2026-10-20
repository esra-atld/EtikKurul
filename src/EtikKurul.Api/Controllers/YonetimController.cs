using EtikKurul.Api.Constants;
using EtikKurul.Api.Data;
using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;
using EtikKurul.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtikKurul.Api.Controllers;

/// <summary>
/// Yetkililerin işlemleri. Başvurunun yolu:
///   1) ARŞİV KAYIT  : gelen başvuruya tarih + sayı verir, arşive ekler, yetkili birime gönderir
///   2) YETKİLİ BİRİM: onaylayıp etik kurula gönderir (aracı)
///   3) ETİK KURUL   : 3 üyeye aynı anda gider; her üye onaylar / onaylamaz
///                     3 oy tamamlanınca karar hesaplanır (oy birliği / oy çokluğu ile kabul / red)
///   4) YETKİLİ BİRİM: kararı başvurana iletir (aracı) → başvuru sonuçlanır
/// Rol adları kodda: EvrakKayit = Arşiv Kayıt, Dekanlik = Yetkili Birim, EtikKurul = Etik Kurul üyesi
/// </summary>
[ApiController]
[Route("api/yonetim")]
[Authorize(Roles = $"{Roller.EvrakKayit},{Roller.Dekanlik},{Roller.EtikKurul},{Roller.Admin}")]
public class YonetimController(
    AppDbContext db,
    UserManager<Kullanici> kullaniciYoneticisi,
    BasvuruPdfServisi pdfServisi,
    DosyaDepolamaServisi depolama) : ControllerBase
{
    public const int EtikKurulUyeSayisi = 3;

    /// <summary>
    /// Görev listeleri. kutu:
    ///   arsiv-gelen (Arşiv Kayıt)  : kayıt bekleyen başvurular
    ///   arsiv       (Arşiv Kayıt)  : arşive kaydedilmiş bütün başvurular (arama: ?arama=)
    ///   birim       (Yetkili Birim): etik kurula gönderilecekler + başvurana iletilecek kararlar
    ///   birim-izlenen (Yetkili Birim): etik kurulda oylamada olanlar
    ///   oy-bekleyen (Etik Kurul)   : oyumu bekleyenler
    ///   oy-verilen  (Etik Kurul)   : oy verdiklerim
    /// </summary>
    [HttpGet("basvurular")]
    public async Task<ActionResult<List<YonetimOzetDto>>> Liste([FromQuery] string kutu, [FromQuery] string? arama)
    {
        var benimId = User.KullaniciId();
        IQueryable<Basvuru> sorgu = db.Basvurular.AsNoTracking()
            .Include(b => b.BasvuranKullanici)
            .Include(b => b.Karar)
            .Include(b => b.Oylar);

        switch (kutu)
        {
            case "arsiv-gelen" when RolVar(Roller.EvrakKayit):
                sorgu = sorgu.Where(b => b.Durum == BasvuruDurumu.EvrakKayitta);
                break;
            case "arsiv" when RolVar(Roller.EvrakKayit):
                sorgu = sorgu.Where(b => b.EvrakKayitNo != null);
                if (!string.IsNullOrWhiteSpace(arama))
                {
                    var a = arama.Trim();
                    sorgu = sorgu.Where(b =>
                        b.EvrakKayitNo!.Contains(a) || b.ArastirmaAdi.Contains(a) ||
                        b.BasvuranKullanici!.Ad.Contains(a) || b.BasvuranKullanici.Soyad.Contains(a));
                }
                break;
            case "birim" when RolVar(Roller.Dekanlik):
                sorgu = sorgu.Where(b => b.Durum == BasvuruDurumu.Dekanlikta || b.Durum == BasvuruDurumu.KararDekanlikta);
                break;
            case "birim-izlenen" when RolVar(Roller.Dekanlik):
                sorgu = sorgu.Where(b => b.Durum == BasvuruDurumu.EtikKurulda);
                break;
            case "oy-bekleyen" when User.IsInRole(Roller.EtikKurul):
                sorgu = sorgu.Where(b => b.Durum == BasvuruDurumu.EtikKurulda &&
                                         b.Oylar.Any(o => o.UyeKullaniciId == benimId && o.Onayladi == null));
                break;
            case "oy-verilen" when User.IsInRole(Roller.EtikKurul):
                sorgu = sorgu.Where(b => b.Oylar.Any(o => o.UyeKullaniciId == benimId && o.Onayladi != null));
                break;
            default:
                return BadRequest(new { mesaj = "Bu listeyi görme yetkiniz yok." });
        }

        var liste = await sorgu
            .OrderByDescending(b => b.EvrakKayitTarihi ?? b.GonderimTarihi)
            .AsSplitQuery()
            .ToListAsync();

        return liste.Select(b => new YonetimOzetDto(
            b.Id,
            b.ArastirmaAdi,
            $"{b.BasvuranKullanici?.Ad} {b.BasvuranKullanici?.Soyad}",
            b.BasvuranKullanici?.CalistigiBirim ?? "",
            b.Durum,
            b.EvrakKayitNo,
            b.EvrakKayitTarihi,
            b.GonderimTarihi,
            b.Karar is null ? null : Etiketler.KararOzeti(b.Karar),
            b.Oylar.FirstOrDefault(o => o.UyeKullaniciId == benimId)?.Onayladi)).ToList();
    }

    /// <summary>Başvurunun bütün bilgileri + yapılabilecek işlemler</summary>
    [HttpGet("basvurular/{id:int}")]
    public async Task<ActionResult<YonetimDetayDto>> Detay(int id)
    {
        var b = await ErisilebilirBasvuru(id, izle: false);
        if (b is null) return NotFound(new { mesaj = "Başvuru bulunamadı veya görme yetkiniz yok." });

        var benimId = User.KullaniciId();
        var benimOyum = b.Oylar.FirstOrDefault(o => o.UyeKullaniciId == benimId);

        var islemler = new List<string>();
        if (b.Durum == BasvuruDurumu.EvrakKayitta && RolVar(Roller.EvrakKayit)) islemler.Add("ArsiveKaydet");
        if (b.Durum == BasvuruDurumu.Dekanlikta && RolVar(Roller.Dekanlik)) islemler.Add("EtikKuruluGonder");
        if (b.Durum == BasvuruDurumu.EtikKurulda && benimOyum is { Onayladi: null }) islemler.Add("OyVer");
        if (b.Durum == BasvuruDurumu.KararDekanlikta && RolVar(Roller.Dekanlik)) islemler.Add("BasvuranaIlet");

        var ekler = b.Ekler
            .OrderBy(e => e.EkTuru?.Sira).ThenBy(e => e.YuklemeTarihi)
            .Select(e => new EkDto(e.Id, e.EkTuruId, e.EkTuru?.Kod ?? "", e.EkTuru?.Ad ?? "",
                e.BasvuruArastirmacisiId, e.OrijinalDosyaAdi, e.BoyutByte, e.YuklemeTarihi))
            .ToList();

        var k = b.BasvuranKullanici!;
        return new YonetimDetayDto(
            BasvuruEslestirici.DetayaCevir(b),
            new BasvuranDto($"{k.Ad} {k.Soyad}", k.CalistigiBirim, k.Email ?? "", k.KurumsalEmail, k.PhoneNumber),
            HareketleriCevir(b, kisiAdlariGosterilsin: true),
            ekler,
            b.Oylar.Count == 0 ? null : new OylamaDurumuDto(
                b.Oylar.Count,
                b.Oylar.Count(o => o.Onayladi != null),
                benimOyum?.Onayladi,
                benimOyum is { Onayladi: null } && b.Durum == BasvuruDurumu.EtikKurulda),
            KararCevir(b.Karar),
            islemler);
    }

    /// <summary>Başvuru paketi PDF'i (form + dilekçe + taahhütname + gönüllü katılım formu)</summary>
    [HttpGet("basvurular/{id:int}/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> Pdf(int id)
    {
        var b = await ErisilebilirBasvuru(id, izle: false);
        if (b is null) return NotFound(new { mesaj = "Başvuru bulunamadı veya görme yetkiniz yok." });

        var ekTurleri = await db.EkTurleri.AsNoTracking().ToListAsync();
        var ekAdlari = EkKuralServisi.Hesapla(b, ekTurleri).Where(e => e.ZorunluMu).Select(e => e.Ad).ToList();
        return File(pdfServisi.Olustur(b, ekAdlari), "application/pdf", $"EtikKurul-Basvuru-{b.Id}.pdf");
    }

    /// <summary>Başvurana ait yüklenmiş (ıslak imzalı) bir belge</summary>
    [HttpGet("basvurular/{id:int}/ekler/{ekId:int}/dosya")]
    [Produces("application/pdf")]
    public async Task<IActionResult> EkIndir(int id, int ekId)
    {
        var b = await ErisilebilirBasvuru(id, izle: false);
        var ek = b?.Ekler.FirstOrDefault(e => e.Id == ekId);
        if (ek is null || !depolama.VarMi(ek.KayitliDosyaAdi))
            return NotFound(new { mesaj = "Belge bulunamadı." });

        return PhysicalFile(depolama.TamYol(ek.KayitliDosyaAdi), "application/pdf", ek.OrijinalDosyaAdi);
    }

    /// <summary>Arşiv kayıt için sıradaki kayıt sayısı önerisi (ör. 2026/0007). Görevli değiştirebilir.</summary>
    [HttpGet("onerilen-kayit-no")]
    [Authorize(Roles = $"{Roller.EvrakKayit},{Roller.Admin}")]
    public async Task<ActionResult<object>> OnerilenKayitNo()
    {
        var onek = $"{TurkiyeSaati.Simdi.Year}/";
        var buYilkiler = await db.Basvurular
            .Where(b => b.EvrakKayitNo != null && b.EvrakKayitNo.StartsWith(onek))
            .Select(b => b.EvrakKayitNo!)
            .ToListAsync();
        var enBuyuk = buYilkiler
            .Select(n => int.TryParse(n[onek.Length..], out var x) ? x : 0)
            .DefaultIfEmpty(0)
            .Max();
        return new { kayitNo = $"{onek}{enBuyuk + 1:D4}" };
    }

    /// <summary>1) ARŞİV KAYIT: tarih + sayı verir, arşive ekler ve yetkili birime gönderir.</summary>
    [HttpPost("basvurular/{id:int}/arsive-kaydet")]
    [Authorize(Roles = $"{Roller.EvrakKayit},{Roller.Admin}")]
    public async Task<IActionResult> ArsiveKaydet(int id, ArsiveKaydetIstegi istek)
    {
        var b = await ErisilebilirBasvuru(id, izle: true);
        if (b is null) return NotFound(new { mesaj = "Başvuru bulunamadı." });
        if (b.Durum != BasvuruDurumu.EvrakKayitta)
            return Conflict(new { mesaj = "Bu başvuru arşiv kaydı beklemiyor." });

        var kayitNo = istek.KayitNo.Trim();
        if (await db.Basvurular.AnyAsync(x => x.EvrakKayitNo == kayitNo && x.Id != id))
            return Conflict(new { mesaj = $"{kayitNo} sayısı başka bir başvuruda kullanılmış." });

        var tarihUtc = istek.KayitTarihi.Kind == DateTimeKind.Utc
            ? istek.KayitTarihi
            : istek.KayitTarihi.ToUniversalTime();

        b.EvrakKayitNo = kayitNo;
        b.EvrakKayitTarihi = tarihUtc;
        IsAkisi.DurumDegistir(b, BasvuruDurumu.Dekanlikta, User.KullaniciId(),
            $"Arşive kaydedildi (Sayı: {kayitNo}, Tarih: {TurkiyeSaati.Cevir(tarihUtc):dd.MM.yyyy HH:mm}) ve yetkili birime gönderildi.");

        return await KaydetVeCevapla("Başvuru arşive kaydedildi ve yetkili birime gönderildi.");
    }

    /// <summary>2) YETKİLİ BİRİM: onaylayıp etik kurulun 3 üyesine aynı anda gönderir.</summary>
    [HttpPost("basvurular/{id:int}/etik-kurula-gonder")]
    [Authorize(Roles = $"{Roller.Dekanlik},{Roller.Admin}")]
    public async Task<IActionResult> EtikKurulaGonder(int id)
    {
        var b = await ErisilebilirBasvuru(id, izle: true);
        if (b is null) return NotFound(new { mesaj = "Başvuru bulunamadı." });
        if (b.Durum != BasvuruDurumu.Dekanlikta)
            return Conflict(new { mesaj = "Bu başvuru yetkili birimde onay beklemiyor." });

        var uyeler = (await kullaniciYoneticisi.GetUsersInRoleAsync(Roller.EtikKurul))
            .Where(u => u.AktifMi)
            .ToList();
        if (uyeler.Count != EtikKurulUyeSayisi)
            return Conflict(new
            {
                mesaj = $"Etik kurulda {EtikKurulUyeSayisi} aktif üye tanımlı olmalı (şu an {uyeler.Count}). " +
                        "Yöneticinin 'Kullanıcılar ve yetkiler' sayfasından üyeleri tanımlaması gerekiyor.",
            });

        var simdi = DateTime.UtcNow;
        foreach (var uye in uyeler)
            b.Oylar.Add(new EtikKurulOyu { UyeKullaniciId = uye.Id, GonderimTarihi = simdi });

        IsAkisi.DurumDegistir(b, BasvuruDurumu.EtikKurulda, User.KullaniciId(),
            $"Yetkili birim onayladı; başvuru etik kurulun {EtikKurulUyeSayisi} üyesine gönderildi.");

        return await KaydetVeCevapla($"Başvuru etik kurulun {EtikKurulUyeSayisi} üyesine gönderildi.");
    }

    /// <summary>
    /// 3) ETİK KURUL ÜYESİ: onaylıyorum / onaylamıyorum.
    /// Üç oy tamamlanınca karar otomatik hesaplanır ve başvuru yetkili birime döner.
    /// </summary>
    [HttpPost("basvurular/{id:int}/oy")]
    [Authorize(Roles = Roller.EtikKurul)]
    public async Task<IActionResult> OyVer(int id, OyIstegi istek)
    {
        var b = await ErisilebilirBasvuru(id, izle: true);
        if (b is null) return NotFound(new { mesaj = "Başvuru bulunamadı." });
        if (b.Durum != BasvuruDurumu.EtikKurulda)
            return Conflict(new { mesaj = "Bu başvuru için oylama açık değil." });

        var oy = b.Oylar.FirstOrDefault(o => o.UyeKullaniciId == User.KullaniciId());
        if (oy is null) return Forbid();
        if (oy.Onayladi is not null)
            return Conflict(new { mesaj = "Bu başvuru için oyunuzu zaten verdiniz." });

        oy.Onayladi = istek.Onayliyorum!.Value;
        oy.OyTarihi = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Üç oy da verildiyse kararı hesapla
        if (b.Oylar.All(o => o.Onayladi is not null) && b.Karar is null)
        {
            var kabul = b.Oylar.Count(o => o.Onayladi == true);
            var red = b.Oylar.Count - kabul;
            b.Karar = new Karar
            {
                KararTuru = kabul > red ? KararTuru.Uygun : KararTuru.UygunDegil,
                OyBirligiMi = kabul == 0 || red == 0,
                KabulOySayisi = kabul,
                RedOySayisi = red,
                KararTarihi = DateTime.UtcNow,
            };
            IsAkisi.DurumDegistir(b, BasvuruDurumu.KararDekanlikta, User.KullaniciId(),
                $"Etik kurul oylaması tamamlandı: {Etiketler.KararOzeti(b.Karar)}. Karar yetkili birime gönderildi.");
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Başka bir üye aynı anda son oyu verip kararı oluşturmuş olabilir; oy zaten kaydedildi.
            }
            return Ok(new { mesaj = "Oyunuz kaydedildi. Oylama tamamlandı, karar yetkili birime gönderildi." });
        }

        return Ok(new { mesaj = "Oyunuz kaydedildi." });
    }

    /// <summary>4) YETKİLİ BİRİM: etik kurul kararını başvurana iletir.</summary>
    [HttpPost("basvurular/{id:int}/basvurana-ilet")]
    [Authorize(Roles = $"{Roller.Dekanlik},{Roller.Admin}")]
    public async Task<IActionResult> BasvuranaIlet(int id)
    {
        var b = await ErisilebilirBasvuru(id, izle: true);
        if (b is null) return NotFound(new { mesaj = "Başvuru bulunamadı." });
        if (b.Durum != BasvuruDurumu.KararDekanlikta || b.Karar is null)
            return Conflict(new { mesaj = "Bu başvuru için iletilecek bir karar yok." });

        IsAkisi.DurumDegistir(b, BasvuruDurumu.Sonuclandi, User.KullaniciId(),
            $"Etik kurul kararı başvurana iletildi: {Etiketler.KararOzeti(b.Karar)}.");

        return await KaydetVeCevapla("Karar başvurana iletildi.");
    }

    // ─────────────────────────── Yardımcılar ───────────────────────────

    /// <summary>Admin her rolün işini yapabilir (yedek olarak).</summary>
    private bool RolVar(string rol) => User.IsInRole(rol) || User.IsInRole(Roller.Admin);

    private async Task<IActionResult> KaydetVeCevapla(string mesaj)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { mesaj = "Başvuru bu arada başka biri tarafından işlendi. Sayfayı yenileyin." });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mesaj = "Kayıt yapılamadı (aynı kayıt sayısı kullanılmış olabilir)." });
        }
        return Ok(new { mesaj });
    }

    /// <summary>
    /// Kullanıcının görebileceği başvuruyu bütün ayrıntılarıyla getirir.
    /// Taslaklar ve gönderilmemiş başvurular yetkililere görünmez.
    /// Etik kurul üyesi sadece kendisine gönderilen başvuruları görür.
    /// </summary>
    private async Task<Basvuru?> ErisilebilirBasvuru(int id, bool izle)
    {
        IQueryable<Basvuru> sorgu = db.Basvurular
            .Include(b => b.BasvuranKullanici)
            .Include(b => b.Arastirmacilar)
            .Include(b => b.VeriToplamaYerleri)
            .Include(b => b.DestekKurumlari)
            .Include(b => b.GiderKalemleri)
            .Include(b => b.IsPaketleri)
            .Include(b => b.Riskler)
            .Include(b => b.LiteraturKaynaklari)
            .Include(b => b.Secimler).ThenInclude(s => s.Secenek)
            .Include(b => b.Ekler).ThenInclude(e => e.EkTuru)
            .Include(b => b.Hareketler).ThenInclude(h => h.IslemYapanKullanici)
            .Include(b => b.Oylar)
            .Include(b => b.Karar)
            .AsSplitQuery();
        if (!izle) sorgu = sorgu.AsNoTracking();

        var b = await sorgu.FirstOrDefaultAsync(x => x.Id == id);
        if (b is null || b.Durum is BasvuruDurumu.Taslak or BasvuruDurumu.ImzaliBelgeBekleniyor)
            return null;

        if (RolVar(Roller.EvrakKayit) || RolVar(Roller.Dekanlik))
            return b;

        var benimId = User.KullaniciId();
        return User.IsInRole(Roller.EtikKurul) && b.Oylar.Any(o => o.UyeKullaniciId == benimId) ? b : null;
    }

    public static List<HareketDto> HareketleriCevir(Basvuru b, bool kisiAdlariGosterilsin) =>
        b.Hareketler
            .OrderBy(h => h.Tarih)
            .Select(h => new HareketDto(
                h.Tarih,
                h.OncekiDurum,
                h.YeniDurum,
                h.Aciklama,
                kisiAdlariGosterilsin && h.IslemYapanKullanici is { } u ? $"{u.Ad} {u.Soyad}" : null))
            .ToList();

    public static KararOzetDto? KararCevir(Karar? k) => k is null
        ? null
        : new KararOzetDto(k.KararTuru, k.OyBirligiMi, k.KabulOySayisi, k.RedOySayisi, k.KararTarihi, Etiketler.KararOzeti(k));
}
