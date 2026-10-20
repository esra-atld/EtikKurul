using EtikKurul.Api.Data;
using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;
using EtikKurul.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtikKurul.Api.Controllers;

/// <summary>
/// Başvurunun belge aşaması:
///   eksikler → kilitle (PDF hazır) → pdf indir → imzalı belgeleri yükle → gönder
/// Kilit açılırsa yüklenen belgeler silinir (imzalı kağıt artık formla uyuşmaz).
/// </summary>
[ApiController]
[Route("api/basvurular/{id:int}")]
[Authorize]
public class BasvuruBelgeController(
    AppDbContext db,
    BasvuruPdfServisi pdfServisi,
    DosyaDepolamaServisi depolama,
    ILogger<BasvuruBelgeController> logger) : ControllerBase
{
    /// <summary>Formda boş bırakılmış zorunlu alanlar (adım numarasıyla). Önizleme adımında gösterilir.</summary>
    [HttpGet("eksikler")]
    public async Task<ActionResult<EksikAlanlarCevabi>> Eksikler(int id)
    {
        var b = await TumuyleGetir(id, izle: false);
        if (b is null) return BulunamadiCevabi();

        var eksikler = BasvuruDogrulayici.EksikleriBul(b);
        return new EksikAlanlarCevabi(eksikler.Count == 0, eksikler);
    }

    /// <summary>
    /// Formu kilitler ve "İmzalı belge bekleniyor" durumuna geçirir. Bundan sonra PDF imzalanmaya hazırdır.
    /// Eksik alan varsa kilitlemez, eksikleri döner.
    /// </summary>
    [HttpPost("kilitle")]
    public async Task<IActionResult> Kilitle(int id)
    {
        var b = await TumuyleGetir(id, izle: true);
        if (b is null) return BulunamadiCevabi();

        if (!BasvuruEslestirici.DuzenlenebilirMi(b))
            return Conflict(new { mesaj = "Bu başvuru zaten kilitli veya gönderilmiş." });

        var eksikler = BasvuruDogrulayici.EksikleriBul(b);
        if (eksikler.Count > 0)
            return BadRequest(new { mesaj = "Formda eksik alanlar var. Çıktı almadan önce tamamlayın.", eksikler });

        b.FormKilitliMi = true;
        IsAkisi.DurumDegistir(b, BasvuruDurumu.ImzaliBelgeBekleniyor, User.KullaniciId(),
            "Form kilitlendi, PDF çıktısı hazırlandı. Islak imzalı belgeler bekleniyor.");
        await db.SaveChangesAsync();

        return Ok(new { mesaj = "Form kilitlendi. PDF'i indirip imzalayın, taratıp yükleyin." });
    }

    /// <summary>
    /// Kilidi açar, formu tekrar düzenlenebilir yapar. YÜKLENMİŞ BÜTÜN BELGELER SİLİNİR,
    /// çünkü imzalanan kağıt artık formdaki bilgilerle uyuşmayacak.
    /// </summary>
    [HttpPost("kilidi-ac")]
    public async Task<IActionResult> KilidiAc(int id)
    {
        var b = await TumuyleGetir(id, izle: true);
        if (b is null) return BulunamadiCevabi();

        if (b.Durum != BasvuruDurumu.ImzaliBelgeBekleniyor)
            return Conflict(new { mesaj = "Sadece gönderilmemiş, imzalı belge bekleyen başvuruların kilidi açılabilir." });

        var silinenSayisi = b.Ekler.Count;
        var silinecekDosyalar = EkleriKaldir(b.Ekler.ToList());

        b.FormKilitliMi = false;
        IsAkisi.DurumDegistir(b, BasvuruDurumu.Taslak, User.KullaniciId(),
            $"Düzenleme için kilit açıldı. {silinenSayisi} yüklenmiş belge silindi.");
        await db.SaveChangesAsync();
        DosyalariSil(silinecekDosyalar);

        return Ok(new { mesaj = "Form tekrar düzenlenebilir. Değişiklikten sonra yeni çıktı alıp tekrar imzalamanız gerekir.", silinenBelgeSayisi = silinenSayisi });
    }

    /// <summary>
    /// Başvuru paketi PDF'i: Dilekçe + Başvuru Formu + Ek-1 Taahhütname + Ek-2 Gönüllü Katılım Formu.
    /// Form kilitli değilse "TASLAK" filigranlı önizleme döner.
    /// </summary>
    [HttpGet("pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> Pdf(int id)
    {
        var b = await TumuyleGetir(id, izle: false);
        if (b is null) return BulunamadiCevabi();

        var ekTurleri = await db.EkTurleri.AsNoTracking().ToListAsync();
        var ekAdlari = EkKuralServisi.Hesapla(b, ekTurleri)
            .Where(e => e.ZorunluMu)
            .Select(e => e.Ad)
            .ToList();

        var pdf = pdfServisi.Olustur(b, ekAdlari);
        var ad = b.FormKilitliMi ? $"EtikKurul-Basvuru-{b.Id}.pdf" : $"EtikKurul-Basvuru-{b.Id}-TASLAK.pdf";
        return File(pdf, "application/pdf", ad);
    }

    /// <summary>Yüklenmiş belgelerin listesi.</summary>
    [HttpGet("ekler")]
    public async Task<ActionResult<List<EkDto>>> Ekler(int id)
    {
        if (!await SahibiMi(id)) return BulunamadiCevabi();

        return await db.Ekler.AsNoTracking()
            .Where(e => e.BasvuruId == id)
            .OrderBy(e => e.EkTuru!.Sira).ThenBy(e => e.YuklemeTarihi)
            .Select(e => new EkDto(e.Id, e.EkTuruId, e.EkTuru!.Kod, e.EkTuru.Ad,
                e.BasvuruArastirmacisiId, e.OrijinalDosyaAdi, e.BoyutByte, e.YuklemeTarihi))
            .ToListAsync();
    }

    /// <summary>
    /// Islak imzalı, taranmış PDF yükler. Tek dosyalık bir ek türü tekrar yüklenirse eskisinin yerine geçer.
    /// Ek-7 (özgeçmiş) için basvuruArastirmacisiId zorunludur.
    /// </summary>
    [HttpPost("ekler")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> EkYukle(int id, [FromForm] EkYuklemeIstegi istek)
    {
        var b = await TumuyleGetir(id, izle: true);
        if (b is null) return BulunamadiCevabi();

        if (b.Durum != BasvuruDurumu.ImzaliBelgeBekleniyor)
            return Conflict(new { mesaj = "Belgeler, form kilitlendikten sonra ve gönderilmeden önce yüklenebilir." });

        // Dosya kontrolleri
        if (istek.Dosya.Length == 0)
            return BadRequest(new { mesaj = "Dosya boş." });
        if (istek.Dosya.Length > depolama.MaksimumBoyutByte)
            return BadRequest(new { mesaj = $"Dosya en fazla {depolama.MaksimumBoyutByte / 1024 / 1024} MB olabilir." });
        if (!await DosyaDepolamaServisi.PdfMiAsync(istek.Dosya))
            return BadRequest(new { mesaj = "Sadece PDF dosyası yüklenebilir." });

        // Bu ek türü bu başvuruda istenebilir mi?
        var ekTurleri = await db.EkTurleri.AsNoTracking().ToListAsync();
        var gerekliler = EkKuralServisi.Hesapla(b, ekTurleri);
        var hedef = gerekliler.FirstOrDefault(g => g.EkTuruId == istek.EkTuruId);
        if (hedef is null)
            return BadRequest(new { mesaj = "Bu belge türü bu başvuru için istenmiyor." });

        var ekTuru = ekTurleri.First(t => t.Id == istek.EkTuruId);
        List<Ek> yerineGecenler;

        if (ekTuru.ZorunlulukKurali == EkZorunlulukKurali.HerArastirmaciIcin)
        {
            if (istek.BasvuruArastirmacisiId is not { } arastirmaciId ||
                b.Arastirmacilar.All(a => a.Id != arastirmaciId))
                return BadRequest(new { mesaj = "Özgeçmişin hangi araştırmacıya ait olduğunu seçin." });

            // Aynı kişinin eski özgeçmişi varsa yenisiyle değiştir
            yerineGecenler = b.Ekler.Where(e => e.EkTuruId == ekTuru.Id && e.BasvuruArastirmacisiId == arastirmaciId).ToList();
        }
        else
        {
            istek.BasvuruArastirmacisiId = null;
            yerineGecenler = ekTuru.CokluDosyaMi
                ? []
                : b.Ekler.Where(e => e.EkTuruId == ekTuru.Id).ToList();
        }

        var kayitliAd = await depolama.KaydetAsync(istek.Dosya);
        List<string> eskiDosyalar;
        try
        {
            eskiDosyalar = EkleriKaldir(yerineGecenler);

            b.Ekler.Add(new Ek
            {
                EkTuruId = ekTuru.Id,
                BasvuruArastirmacisiId = istek.BasvuruArastirmacisiId,
                OrijinalDosyaAdi = Path.GetFileName(istek.Dosya.FileName),
                KayitliDosyaAdi = kayitliAd,
                BoyutByte = istek.Dosya.Length,
                IcerikTuru = "application/pdf",
                YukleyenKullaniciId = User.KullaniciId(),
                YuklemeTarihi = DateTime.UtcNow,
            });
            b.GuncellemeTarihi = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        catch
        {
            depolama.Sil(kayitliAd); // Veritabanına yazılamadıysa diskte sahipsiz dosya kalmasın
            throw;
        }
        DosyalariSil(eskiDosyalar); // Yenisi kaydedildi, eskisini artık silebiliriz

        return Ok(new { mesaj = $"{ekTuru.Ad} yüklendi." });
    }

    /// <summary>Yüklenmiş belgeyi indirir.</summary>
    [HttpGet("ekler/{ekId:int}/dosya")]
    [Produces("application/pdf")]
    public async Task<IActionResult> EkIndir(int id, int ekId)
    {
        if (!await SahibiMi(id)) return BulunamadiCevabi();

        var ek = await db.Ekler.AsNoTracking().FirstOrDefaultAsync(e => e.Id == ekId && e.BasvuruId == id);
        if (ek is null || !depolama.VarMi(ek.KayitliDosyaAdi))
            return NotFound(new { mesaj = "Belge bulunamadı." });

        return PhysicalFile(depolama.TamYol(ek.KayitliDosyaAdi), "application/pdf", ek.OrijinalDosyaAdi);
    }

    /// <summary>Yüklenmiş belgeyi siler (gönderilmeden önce).</summary>
    [HttpDelete("ekler/{ekId:int}")]
    public async Task<IActionResult> EkSil(int id, int ekId)
    {
        var b = await TumuyleGetir(id, izle: true);
        if (b is null) return BulunamadiCevabi();

        if (b.Durum != BasvuruDurumu.ImzaliBelgeBekleniyor)
            return Conflict(new { mesaj = "Gönderilmiş başvurunun belgeleri silinemez." });

        var ek = b.Ekler.FirstOrDefault(e => e.Id == ekId);
        if (ek is null) return NotFound(new { mesaj = "Belge bulunamadı." });

        var dosyalar = EkleriKaldir([ek]);
        b.GuncellemeTarihi = DateTime.UtcNow;
        await db.SaveChangesAsync();
        DosyalariSil(dosyalar);
        return NoContent();
    }

    /// <summary>
    /// Başvuruyu gönderir: zorunlu belgelerin hepsi yüklendiyse başvuru Evrak Kayıt'a düşer.
    /// </summary>
    [HttpPost("gonder")]
    public async Task<IActionResult> Gonder(int id)
    {
        var b = await TumuyleGetir(id, izle: true);
        if (b is null) return BulunamadiCevabi();

        if (b.Durum != BasvuruDurumu.ImzaliBelgeBekleniyor)
            return Conflict(new { mesaj = "Başvuru gönderilmeye hazır değil veya zaten gönderilmiş." });

        var ekTurleri = await db.EkTurleri.AsNoTracking().ToListAsync();
        var eksikBelgeler = EkKuralServisi.Hesapla(b, ekTurleri)
            .Where(g => g.ZorunluMu && !g.TamamMi)
            .Select(g => g.GerekenAdet > 1 ? $"{g.Ad} ({g.YuklenenAdet}/{g.GerekenAdet})" : g.Ad)
            .ToList();
        if (eksikBelgeler.Count > 0)
            return BadRequest(new { mesaj = "Zorunlu belgelerin hepsi yüklenmedi.", eksikBelgeler });

        b.GonderimTarihi = DateTime.UtcNow;
        IsAkisi.DurumDegistir(b, BasvuruDurumu.EvrakKayitta, User.KullaniciId(),
            "Başvuru gönderildi, arşiv kaydı bekleniyor.");
        await db.SaveChangesAsync();

        // TODO (e-posta adımı): Evrak Kayıt ve Etik Kurul'a bildirim maili gönder.
        logger.LogInformation("Başvuru {Id} gönderildi.", b.Id);

        return Ok(new { mesaj = "Başvurunuz gönderildi. Durumunu 'Başvurularım' sayfasından takip edebilirsiniz." });
    }

    // ─────────────────────────── Yardımcılar ───────────────────────────

    private ObjectResult BulunamadiCevabi() => NotFound(new { mesaj = "Başvuru bulunamadı." });

    private Task<bool> SahibiMi(int id)
    {
        var kullaniciId = User.KullaniciId();
        return db.Basvurular.AnyAsync(b => b.Id == id && b.BasvuranKullaniciId == kullaniciId);
    }

    /// <summary>Başvuruyu bütün alt tablolarıyla getirir. Kullanıcıya ait değilse null.</summary>
    private async Task<Basvuru?> TumuyleGetir(int id, bool izle)
    {
        var kullaniciId = User.KullaniciId();
        IQueryable<Basvuru> sorgu = db.Basvurular
            .Where(b => b.Id == id && b.BasvuranKullaniciId == kullaniciId)
            .Include(b => b.Arastirmacilar)
            .Include(b => b.VeriToplamaYerleri)
            .Include(b => b.DestekKurumlari)
            .Include(b => b.GiderKalemleri)
            .Include(b => b.IsPaketleri)
            .Include(b => b.Riskler)
            .Include(b => b.LiteraturKaynaklari)
            .Include(b => b.Secimler).ThenInclude(s => s.Secenek)
            .Include(b => b.Ekler)
            .AsSplitQuery();

        if (!izle)
            sorgu = sorgu.AsNoTracking();

        return await sorgu.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Ek kayıtlarını silinmek üzere işaretler ve diskteki dosya adlarını döner.
    /// Dosyalar, SaveChanges BAŞARILI olduktan sonra DosyalariSil ile silinmeli
    /// (veritabanı hata verirse dosyalar kaybolmasın diye).
    /// </summary>
    private List<string> EkleriKaldir(List<Ek> ekler)
    {
        db.Ekler.RemoveRange(ekler);
        return ekler.Select(e => e.KayitliDosyaAdi).ToList();
    }

    private void DosyalariSil(IEnumerable<string> dosyaAdlari)
    {
        foreach (var ad in dosyaAdlari)
        {
            try { depolama.Sil(ad); }
            catch (IOException ex) { logger.LogWarning(ex, "Dosya silinemedi: {Dosya}", ad); }
        }
    }
}
