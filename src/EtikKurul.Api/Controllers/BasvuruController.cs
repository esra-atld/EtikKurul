using EtikKurul.Api.Data;
using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;
using EtikKurul.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtikKurul.Api.Controllers;

/// <summary>
/// Başvuranın kendi başvuruları: oluşturma, taslak kaydetme (kaldığı yerden devam),
/// görüntüleme, silme ve gerekli eklerin listesi.
/// Her kullanıcı yalnızca KENDİ başvurularını görür.
/// </summary>
[ApiController]
[Route("api/basvurular")]
[Authorize]
public class BasvuruController(AppDbContext db) : ControllerBase
{
    /// <summary>Giriş yapmış kullanıcının başvuruları (en son güncellenen en üstte).</summary>
    [HttpGet]
    public async Task<ActionResult<List<BasvuruOzetDto>>> Benimkiler()
    {
        var kullaniciId = User.KullaniciId();
        var liste = await db.Basvurular.AsNoTracking()
            .Include(b => b.Karar)
            .Where(b => b.BasvuranKullaniciId == kullaniciId)
            .OrderByDescending(b => b.GuncellemeTarihi)
            .ToListAsync();

        return liste.Select(BasvuruEslestirici.OzeteCevir).ToList();
    }

    /// <summary>Yeni taslak başvuru oluşturur. Wizard'ın ilk adımı.</summary>
    [HttpPost]
    [ProducesResponseType<BasvuruDetayDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Olustur(BasvuruOlusturIstegi istek)
    {
        var kullaniciId = User.KullaniciId();

        if (istek.UstBasvuruId is { } ustId &&
            !await db.Basvurular.AnyAsync(b => b.Id == ustId && b.BasvuranKullaniciId == kullaniciId))
        {
            return BadRequest(new { mesaj = "Önceki başvuru bulunamadı veya size ait değil." });
        }

        var simdi = DateTime.UtcNow;
        var basvuru = new Basvuru
        {
            BasvuranKullaniciId = kullaniciId,
            ArastirmaAdi = istek.ArastirmaAdi?.Trim() ?? string.Empty,
            BasvuruTuru = istek.BasvuruTuru,
            UstBasvuruId = istek.UstBasvuruId,
            Durum = BasvuruDurumu.Taslak,
            OlusturmaTarihi = simdi,
            GuncellemeTarihi = simdi,
        };

        basvuru.Hareketler.Add(new BasvuruHareketi
        {
            OncekiDurum = null,
            YeniDurum = BasvuruDurumu.Taslak,
            IslemYapanKullaniciId = kullaniciId,
            Tarih = simdi,
            Aciklama = "Başvuru oluşturuldu.",
        });

        db.Basvurular.Add(basvuru);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Getir), new { id = basvuru.Id }, BasvuruEslestirici.DetayaCevir(basvuru));
    }

    /// <summary>Başvurunun tüm bilgileri. Wizard açılırken kaldığı yerden doldurmak için.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BasvuruDetayDto>> Getir(int id)
    {
        var basvuru = await TumuyleGetir(id, izle: false);
        if (basvuru is null || basvuru.BasvuranKullaniciId != User.KullaniciId())
            return NotFound(new { mesaj = "Başvuru bulunamadı." });

        return BasvuruEslestirici.DetayaCevir(basvuru);
    }

    /// <summary>
    /// Taslağı kaydeder. Wizard'ın her "Kaydet / İleri" tıklamasında formun TAMAMI gönderilir.
    /// Sadece Taslak veya İade Edildi durumundaki, kilitli olmayan başvurular değiştirilebilir.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BasvuruDetayDto>> Kaydet(int id, BasvuruFormuDto form)
    {
        var kullaniciId = User.KullaniciId();
        var basvuru = await TumuyleGetir(id, izle: true);
        if (basvuru is null || basvuru.BasvuranKullaniciId != kullaniciId)
            return NotFound(new { mesaj = "Başvuru bulunamadı." });

        if (!BasvuruEslestirici.DuzenlenebilirMi(basvuru))
            return Conflict(new { mesaj = "Bu başvuru artık düzenlenemez (gönderilmiş veya kilitlenmiş)." });

        if (form.UstBasvuruId is { } ustId && ustId != basvuru.UstBasvuruId)
        {
            if (ustId == basvuru.Id ||
                !await db.Basvurular.AnyAsync(b => b.Id == ustId && b.BasvuranKullaniciId == kullaniciId))
                return BadRequest(new { mesaj = "Önceki başvuru bulunamadı veya size ait değil." });
        }
        basvuru.UstBasvuruId = form.UstBasvuruId;

        // 1) Tek değerli alanlar
        BasvuruEslestirici.TekDegerliAlanlariUygula(basvuru, form);

        // 2) Araştırmacılar: Id ile eşleştirerek güncelle (özgeçmiş ekleri bağlı kalsın)
        await ArastirmacilariGuncelle(basvuru, form.Arastirmacilar);

        // 3) Çoklu seçimler: farkı uygula
        await SecimleriGuncelle(basvuru, form.SecenekIdleri);

        // 4) Diğer listeler: tamamen yenile (dışarıdan bağlı kayıt yok)
        basvuru.VeriToplamaYerleri.Clear();
        basvuru.VeriToplamaYerleri.AddRange(form.VeriToplamaYerleri.Select((y, i) =>
            new VeriToplamaYeri { Ad = y.Ad.Trim(), Sira = i + 1 }));

        basvuru.DestekKurumlari.Clear();
        basvuru.DestekKurumlari.AddRange(form.DestekKurumlari.Select(d =>
            new BasvuruDestekKurumu { Durum = d.Durum, KurumTuru = d.KurumTuru, Aciklama = d.Aciklama?.Trim() }));

        basvuru.GiderKalemleri.Clear();
        basvuru.GiderKalemleri.AddRange(form.GiderKalemleri.Select((g, i) =>
            new GiderKalemi { Tur = g.Tur, Aciklama = g.Aciklama.Trim(), Tutar = g.Tutar, Sira = i + 1 }));

        basvuru.IsPaketleri.Clear();
        basvuru.IsPaketleri.AddRange(form.IsPaketleri.Select(p =>
            new IsPaketi { No = p.No, Ad = p.Ad.Trim(), Kimler = p.Kimler?.Trim(), Sure = p.Sure?.Trim() }));

        basvuru.Riskler.Clear();
        basvuru.Riskler.AddRange(form.Riskler.Select(r =>
            new Risk { IsPaketiNo = r.IsPaketiNo, RiskAciklamasi = r.RiskAciklamasi.Trim(), BPlani = r.BPlani?.Trim() }));

        basvuru.LiteraturKaynaklari.Clear();
        basvuru.LiteraturKaynaklari.AddRange(form.LiteraturKaynaklari.Select((l, i) =>
            new LiteraturKaynagi { Tur = l.Tur, Kaynak = l.Kaynak.Trim(), Sira = i + 1 }));

        basvuru.GuncellemeTarihi = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { mesaj = "Başvuru başka bir pencerede değiştirilmiş. Sayfayı yenileyip tekrar deneyin." });
        }

        return BasvuruEslestirici.DetayaCevir(basvuru);
    }

    /// <summary>Sadece hiç gönderilmemiş taslaklar silinebilir.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var basvuru = await db.Basvurular.Include(b => b.Ekler)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (basvuru is null || basvuru.BasvuranKullaniciId != User.KullaniciId())
            return NotFound(new { mesaj = "Başvuru bulunamadı." });

        if (basvuru.Durum != BasvuruDurumu.Taslak || basvuru.FormKilitliMi)
            return Conflict(new { mesaj = "Sadece taslak durumundaki başvurular silinebilir." });

        // Özgeçmiş ekleri araştırmacıya NoAction ile bağlı; önce ekleri silelim.
        // TODO (dosya yükleme adımında): diskteki dosyaları da sil.
        db.Ekler.RemoveRange(basvuru.Ekler);
        db.Basvurular.Remove(basvuru);
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Bu başvuru için hangi eklerin gerektiği ve kaçının yüklendiği.</summary>
    [HttpGet("{id:int}/gerekli-ekler")]
    public async Task<ActionResult<List<GerekliEkDto>>> GerekliEkler(int id)
    {
        var basvuru = await db.Basvurular.AsNoTracking()
            .Include(b => b.Arastirmacilar)
            .Include(b => b.Secimler).ThenInclude(s => s.Secenek)
            .Include(b => b.DestekKurumlari)
            .Include(b => b.Ekler)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (basvuru is null || basvuru.BasvuranKullaniciId != User.KullaniciId())
            return NotFound(new { mesaj = "Başvuru bulunamadı." });

        var ekTurleri = await db.EkTurleri.AsNoTracking().ToListAsync();
        return EkKuralServisi.Hesapla(basvuru, ekTurleri);
    }

    /// <summary>
    /// "Başvurum nerede?": başvurunun geçtiği bütün aşamalar ve (iletildiyse) etik kurul kararı.
    /// Yetkililerin adları başvurana gösterilmez.
    /// </summary>
    [HttpGet("{id:int}/takip")]
    public async Task<ActionResult<TakipDto>> Takip(int id)
    {
        var b = await db.Basvurular.AsNoTracking()
            .Include(x => x.Hareketler)
            .Include(x => x.Karar)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (b is null || b.BasvuranKullaniciId != User.KullaniciId())
            return NotFound(new { mesaj = "Başvuru bulunamadı." });

        var kararIletildi = b.Durum is BasvuruDurumu.KararDekanlikta or BasvuruDurumu.Sonuclandi or BasvuruDurumu.Arsivlendi;
        return new TakipDto(
            b.Durum,
            b.EvrakKayitNo,
            b.EvrakKayitTarihi,
            YonetimController.HareketleriCevir(b, kisiAdlariGosterilsin: false),
            kararIletildi ? YonetimController.KararCevir(b.Karar) : null);
    }

    // ─────────────────────────── Yardımcılar ───────────────────────────

    private async Task<Basvuru?> TumuyleGetir(int id, bool izle)
    {
        IQueryable<Basvuru> sorgu = db.Basvurular
            .Include(b => b.Arastirmacilar)
            .Include(b => b.VeriToplamaYerleri)
            .Include(b => b.DestekKurumlari)
            .Include(b => b.GiderKalemleri)
            .Include(b => b.IsPaketleri)
            .Include(b => b.Riskler)
            .Include(b => b.LiteraturKaynaklari)
            .Include(b => b.Secimler)
            .AsSplitQuery(); // Tek dev sorgu yerine tablo başına bir sorgu (daha hızlı)

        if (!izle)
            sorgu = sorgu.AsNoTracking();

        return await sorgu.FirstOrDefaultAsync(b => b.Id == id);
    }

    private async Task ArastirmacilariGuncelle(Basvuru basvuru, List<ArastirmaciDto> gelenler)
    {
        var gelenIdler = gelenler.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

        // Formdan çıkarılan araştırmacılar → (varsa) özgeçmiş ekleriyle birlikte sil
        var silinecekler = basvuru.Arastirmacilar.Where(a => !gelenIdler.Contains(a.Id)).ToList();
        if (silinecekler.Count > 0)
        {
            var silinecekIdler = silinecekler.Select(a => a.Id).ToList();
            var bagliEkler = await db.Ekler
                .Where(e => e.BasvuruArastirmacisiId != null && silinecekIdler.Contains(e.BasvuruArastirmacisiId.Value))
                .ToListAsync();
            db.Ekler.RemoveRange(bagliEkler);

            foreach (var a in silinecekler)
                basvuru.Arastirmacilar.Remove(a);
        }

        // Var olanları güncelle, yenileri ekle
        for (var i = 0; i < gelenler.Count; i++)
        {
            var g = gelenler[i];
            var arastirmaci = g.Id > 0 ? basvuru.Arastirmacilar.FirstOrDefault(a => a.Id == g.Id) : null;
            if (arastirmaci is null)
            {
                arastirmaci = new BasvuruArastirmacisi();
                basvuru.Arastirmacilar.Add(arastirmaci);
            }

            arastirmaci.Rol = g.Rol;
            arastirmaci.Unvan = g.Unvan?.Trim();
            arastirmaci.AdSoyad = g.AdSoyad.Trim();
            arastirmaci.Telefon = g.Telefon?.Trim();
            arastirmaci.Email = g.Email?.Trim();
            arastirmaci.GorevYeriAdres = g.GorevYeriAdres?.Trim();
            arastirmaci.Sira = i + 1;
        }
    }

    private async Task SecimleriGuncelle(Basvuru basvuru, List<int> gelenIdler)
    {
        // Sadece var olan ve aktif seçenekleri kabul et
        var istenen = gelenIdler.Distinct().ToList();
        var gecerliIdler = (await db.Secenekler
                .Where(s => s.AktifMi && istenen.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync())
            .ToHashSet();

        var kaldirilacaklar = basvuru.Secimler.Where(s => !gecerliIdler.Contains(s.SecenekId)).ToList();
        foreach (var s in kaldirilacaklar)
            basvuru.Secimler.Remove(s);

        var mevcutlar = basvuru.Secimler.Select(s => s.SecenekId).ToHashSet();
        foreach (var secenekId in gecerliIdler.Where(x => !mevcutlar.Contains(x)))
            basvuru.Secimler.Add(new BasvuruSecenek { BasvuruId = basvuru.Id, SecenekId = secenekId });
    }
}
