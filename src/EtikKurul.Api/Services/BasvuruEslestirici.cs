using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Services;

/// <summary>Basvuru (veritabanı) ↔ DTO (frontend) dönüşümleri.</summary>
public static class BasvuruEslestirici
{
    /// <summary>Form verisini başvurunun TEK DEĞERLİ alanlarına yazar. (Listeler controller'da güncellenir.)</summary>
    public static void TekDegerliAlanlariUygula(Basvuru b, BasvuruFormuDto f)
    {
        b.SonAdim = Math.Max(b.SonAdim, f.SonAdim);
        b.DeneyVeyaBiyolojikMateryalVarMi = f.DeneyVeyaBiyolojikMateryalVarMi;

        b.ArastirmaAdi = f.ArastirmaAdi?.Trim() ?? string.Empty;
        b.BasvuruTuru = f.BasvuruTuru;
        b.ArastirmaNiteligi = f.ArastirmaNiteligi;
        b.ArastirmaNiteligiDiger = Temizle(f.ArastirmaNiteligiDiger);
        b.BagliKurum = Temizle(f.BagliKurum);

        b.VeriToplamaAraciDiger = Temizle(f.VeriToplamaAraciDiger);
        b.VeriToplamaBaslangic = f.VeriToplamaBaslangic;
        b.VeriToplamaBitis = f.VeriToplamaBitis;
        b.TahminiSureAy = f.TahminiSureAy;
        b.VeriAraciBaskasinaAitMi = f.VeriAraciBaskasinaAitMi;

        b.TurkceOzet = Temizle(f.TurkceOzet);
        b.AnahtarKelimeler = Temizle(f.AnahtarKelimeler);
        b.YabanciDilOzet = Temizle(f.YabanciDilOzet);
        b.Keywords = Temizle(f.Keywords);
        b.Amac = Temizle(f.Amac);
        b.KonuKapsamLiteratur = Temizle(f.KonuKapsamLiteratur);
        b.MateryalYontem = Temizle(f.MateryalYontem);
        b.OzgunDeger = Temizle(f.OzgunDeger);
        b.YayginEtki = Temizle(f.YayginEtki);

        b.YanlisBilgiVeyaGizlilikGerekiyorMu = f.YanlisBilgiVeyaGizlilikGerekiyorMu;
        b.YanlisBilgiVeyaGizlilikAciklama = Temizle(f.YanlisBilgiVeyaGizlilikAciklama);
        b.SaglikTehdidiVarMi = f.SaglikTehdidiVarMi;
        b.SaglikTehdidiAciklama = Temizle(f.SaglikTehdidiAciklama);
        b.KisilikHaklariKorunuyorMu = f.KisilikHaklariKorunuyorMu;
        b.KisilikHaklariAciklama = Temizle(f.KisilikHaklariAciklama);
        b.ResitOlmayanKatilimciVarMi = f.ResitOlmayanKatilimciVarMi;
        b.ResitOlmayanKatilimciAciklama = Temizle(f.ResitOlmayanKatilimciAciklama);
        b.YeterliBilgiVeriliyorMu = f.YeterliBilgiVeriliyorMu;
        b.YeterliBilgiAciklama = Temizle(f.YeterliBilgiAciklama);
        b.GonulluKatilimOnlemleri = Temizle(f.GonulluKatilimOnlemleri);
        b.KatilmaCikmaKosullari = Temizle(f.KatilmaCikmaKosullari);
        b.BeklenenKatilimciSayisi = f.BeklenenKatilimciSayisi;
        b.KatilimciGrubuDiger = Temizle(f.KatilimciGrubuDiger);
        b.UygulamaDiger = Temizle(f.UygulamaDiger);
    }

    public static BasvuruDetayDto DetayaCevir(Basvuru b) => new()
    {
        Id = b.Id,
        Durum = b.Durum,
        FormKilitliMi = b.FormKilitliMi,
        DuzenlenebilirMi = DuzenlenebilirMi(b),
        EvrakKayitNo = b.EvrakKayitNo,
        EvrakKayitTarihi = b.EvrakKayitTarihi,
        ToplamGider = b.GiderKalemleri.Sum(g => g.Tutar),
        OlusturmaTarihi = b.OlusturmaTarihi,
        GuncellemeTarihi = b.GuncellemeTarihi,
        GonderimTarihi = b.GonderimTarihi,

        SonAdim = b.SonAdim,
        DeneyVeyaBiyolojikMateryalVarMi = b.DeneyVeyaBiyolojikMateryalVarMi,
        ArastirmaAdi = b.ArastirmaAdi,
        BasvuruTuru = b.BasvuruTuru,
        ArastirmaNiteligi = b.ArastirmaNiteligi,
        ArastirmaNiteligiDiger = b.ArastirmaNiteligiDiger,
        BagliKurum = b.BagliKurum,
        UstBasvuruId = b.UstBasvuruId,
        VeriToplamaAraciDiger = b.VeriToplamaAraciDiger,
        VeriToplamaBaslangic = b.VeriToplamaBaslangic,
        VeriToplamaBitis = b.VeriToplamaBitis,
        TahminiSureAy = b.TahminiSureAy,
        VeriAraciBaskasinaAitMi = b.VeriAraciBaskasinaAitMi,
        TurkceOzet = b.TurkceOzet,
        AnahtarKelimeler = b.AnahtarKelimeler,
        YabanciDilOzet = b.YabanciDilOzet,
        Keywords = b.Keywords,
        Amac = b.Amac,
        KonuKapsamLiteratur = b.KonuKapsamLiteratur,
        MateryalYontem = b.MateryalYontem,
        OzgunDeger = b.OzgunDeger,
        YayginEtki = b.YayginEtki,
        YanlisBilgiVeyaGizlilikGerekiyorMu = b.YanlisBilgiVeyaGizlilikGerekiyorMu,
        YanlisBilgiVeyaGizlilikAciklama = b.YanlisBilgiVeyaGizlilikAciklama,
        SaglikTehdidiVarMi = b.SaglikTehdidiVarMi,
        SaglikTehdidiAciklama = b.SaglikTehdidiAciklama,
        KisilikHaklariKorunuyorMu = b.KisilikHaklariKorunuyorMu,
        KisilikHaklariAciklama = b.KisilikHaklariAciklama,
        ResitOlmayanKatilimciVarMi = b.ResitOlmayanKatilimciVarMi,
        ResitOlmayanKatilimciAciklama = b.ResitOlmayanKatilimciAciklama,
        YeterliBilgiVeriliyorMu = b.YeterliBilgiVeriliyorMu,
        YeterliBilgiAciklama = b.YeterliBilgiAciklama,
        GonulluKatilimOnlemleri = b.GonulluKatilimOnlemleri,
        KatilmaCikmaKosullari = b.KatilmaCikmaKosullari,
        BeklenenKatilimciSayisi = b.BeklenenKatilimciSayisi,
        KatilimciGrubuDiger = b.KatilimciGrubuDiger,
        UygulamaDiger = b.UygulamaDiger,

        SecenekIdleri = b.Secimler.Select(s => s.SecenekId).Order().ToList(),
        Arastirmacilar = b.Arastirmacilar.OrderBy(a => a.Sira).Select(a => new ArastirmaciDto
        {
            Id = a.Id, Rol = a.Rol, Unvan = a.Unvan, AdSoyad = a.AdSoyad,
            Telefon = a.Telefon, Email = a.Email, GorevYeriAdres = a.GorevYeriAdres,
        }).ToList(),
        VeriToplamaYerleri = b.VeriToplamaYerleri.OrderBy(y => y.Sira)
            .Select(y => new VeriToplamaYeriDto { Ad = y.Ad }).ToList(),
        DestekKurumlari = b.DestekKurumlari
            .Select(d => new DestekKurumuDto { Durum = d.Durum, KurumTuru = d.KurumTuru, Aciklama = d.Aciklama }).ToList(),
        GiderKalemleri = b.GiderKalemleri.OrderBy(g => g.Sira)
            .Select(g => new GiderKalemiDto { Tur = g.Tur, Aciklama = g.Aciklama, Tutar = g.Tutar }).ToList(),
        IsPaketleri = b.IsPaketleri.OrderBy(i => i.No)
            .Select(i => new IsPaketiDto { No = i.No, Ad = i.Ad, Kimler = i.Kimler, Sure = i.Sure }).ToList(),
        Riskler = b.Riskler.OrderBy(r => r.IsPaketiNo)
            .Select(r => new RiskDto { IsPaketiNo = r.IsPaketiNo, RiskAciklamasi = r.RiskAciklamasi, BPlani = r.BPlani }).ToList(),
        LiteraturKaynaklari = b.LiteraturKaynaklari.OrderBy(l => l.Tur).ThenBy(l => l.Sira)
            .Select(l => new LiteraturDto { Tur = l.Tur, Kaynak = l.Kaynak }).ToList(),
    };

    public static BasvuruOzetDto OzeteCevir(Basvuru b) => new(
        b.Id, b.ArastirmaAdi, b.BasvuruTuru, b.Durum, b.SonAdim,
        b.EvrakKayitNo, b.OlusturmaTarihi, b.GuncellemeTarihi, b.GonderimTarihi,
        // Etik kurul oylaması bitince karar başvurana da görünür (yetkili birim iletmeden önce de)
        b.Karar is not null && b.Durum is BasvuruDurumu.KararDekanlikta or BasvuruDurumu.Sonuclandi or BasvuruDurumu.Arsivlendi
            ? Etiketler.KararOzeti(b.Karar)
            : null);

    /// <summary>Başvuran formu sadece taslakken veya iade edildiğinde, kilitli değilse değiştirebilir.</summary>
    public static bool DuzenlenebilirMi(Basvuru b) =>
        !b.FormKilitliMi && b.Durum is BasvuruDurumu.Taslak or BasvuruDurumu.IadeEdildi;

    private static string? Temizle(string? metin) =>
        string.IsNullOrWhiteSpace(metin) ? null : metin.Trim();
}
