using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Services;

/// <summary>Eksik/hatalı alan. Adim: kullanıcının düzeltmesi için wizard'da gideceği adım.</summary>
public record EksikAlan(int Adim, string Alan, string Mesaj);

/// <summary>
/// Taslakta her şey boş olabilir; ama PDF çıktısı alınıp form kilitlenmeden önce
/// zorunlu alanların hepsi dolu olmalı. Bu sınıf eksikleri adım adım listeler.
///
/// WIZARD ADIMLARI (frontend bu numaraları kullanacak):
///  1 Ön kontrol ve genel bilgiler     5 Proje içeriği (özet, amaç, literatür, yöntem...)
///  2 Araştırmacılar                   6 İş paketleri ve riskler
///  3 Veri toplama                     7 Etik sorular ve katılımcılar
///  4 Destek ve giderler               8 Önizleme ve çıktı   9 Belge yükleme ve gönderme
/// </summary>
public static class BasvuruDogrulayici
{
    /// <param name="b">Arastirmacilar ve Secimler(+Secenek) yüklenmiş olmalı.</param>
    public static List<EksikAlan> EksikleriBul(Basvuru b)
    {
        var e = new List<EksikAlan>();

        // ───── 1. Ön kontrol ve genel bilgiler ─────
        if (b.DeneyVeyaBiyolojikMateryalVarMi is null)
            e.Add(new(1, nameof(b.DeneyVeyaBiyolojikMateryalVarMi), "İnsan/hayvan deneyi veya biyolojik materyal sorusunu cevaplayın."));
        else if (b.DeneyVeyaBiyolojikMateryalVarMi == true)
            e.Add(new(1, nameof(b.DeneyVeyaBiyolojikMateryalVarMi),
                "Deney niteliği taşıyan veya biyolojik materyal kullanılan çalışmalar için Sağlık, Deney Hayvanları veya Klinik Etik Kurula başvurmalısınız."));

        if (Bos(b.ArastirmaAdi))
            e.Add(new(1, nameof(b.ArastirmaAdi), "Araştırmanın adı zorunludur."));
        if (b.ArastirmaNiteligi is null)
            e.Add(new(1, nameof(b.ArastirmaNiteligi), "Araştırmanın niteliğini seçin."));
        else if (b.ArastirmaNiteligi == ArastirmaNiteligi.Diger && Bos(b.ArastirmaNiteligiDiger))
            e.Add(new(1, nameof(b.ArastirmaNiteligiDiger), "Araştırmanın niteliği 'Diğer' ise açıklayın."));
        if (Bos(b.BagliKurum))
            e.Add(new(1, nameof(b.BagliKurum), "Sorumlu araştırmacının bağlı olduğu kurumu yazın."));
        if (b.BasvuruTuru != BasvuruTuru.YeniBasvuru && b.UstBasvuruId is null)
            e.Add(new(1, nameof(b.UstBasvuruId), "Revizyon/devam başvurusunda önceki başvuruyu seçin."));

        // ───── 2. Araştırmacılar ─────
        var sorumlular = b.Arastirmacilar.Where(a => a.Rol == ArastirmaciRolu.SorumluArastirmaci).ToList();
        if (sorumlular.Count == 0)
            e.Add(new(2, "Arastirmacilar", "Bir sorumlu araştırmacı (proje yürütücüsü/danışman) ekleyin."));
        else if (sorumlular.Count > 1)
            e.Add(new(2, "Arastirmacilar", "Yalnızca bir sorumlu araştırmacı olabilir."));
        foreach (var s in sorumlular)
        {
            if (Bos(s.Telefon) || Bos(s.Email) || Bos(s.GorevYeriAdres))
                e.Add(new(2, "Arastirmacilar", $"Sorumlu araştırmacı ({s.AdSoyad}) için telefon, e-posta ve görev yeri zorunludur."));
        }
        if (b.ArastirmaNiteligi is ArastirmaNiteligi.YuksekLisansTezi or ArastirmaNiteligi.DoktoraSanattaYeterlilikTezi
                or ArastirmaNiteligi.TezsizYuksekLisansProjesi
            && b.Arastirmacilar.All(a => a.Rol != ArastirmaciRolu.IkinciDanismanTezYazari))
            e.Add(new(2, "Arastirmacilar", "Tez çalışmalarında tez yazarını ekleyin (taahhütnamede imzası gerekir)."));

        // ───── 3. Veri toplama ─────
        var veriAraclari = Secilenler(b, SecenekGrubu.VeriToplamaAraci);
        if (veriAraclari.Count == 0)
            e.Add(new(3, "SecenekIdleri", "En az bir veri toplama aracı türü seçin."));
        if (veriAraclari.Any(s => s.AciklamaGerekirMi) && Bos(b.VeriToplamaAraciDiger))
            e.Add(new(3, nameof(b.VeriToplamaAraciDiger), "Veri toplama aracı 'Diğer' ise açıklayın."));
        if (b.VeriToplamaBaslangic is null || b.VeriToplamaBitis is null)
            e.Add(new(3, nameof(b.VeriToplamaBaslangic), "Veri toplama başlangıç ve bitiş tarihlerini girin."));
        else if (b.VeriToplamaBitis < b.VeriToplamaBaslangic)
            e.Add(new(3, nameof(b.VeriToplamaBitis), "Bitiş tarihi başlangıç tarihinden önce olamaz."));
        if (b.TahminiSureAy is null or <= 0)
            e.Add(new(3, nameof(b.TahminiSureAy), "Tahmini araştırma süresini (ay) girin."));
        if (b.VeriToplamaYerleri.Count == 0)
            e.Add(new(3, "VeriToplamaYerleri", "Veri toplanacak en az bir yer/kurum ekleyin."));
        if (b.VeriAraciBaskasinaAitMi is null)
            e.Add(new(3, nameof(b.VeriAraciBaskasinaAitMi), "Veri toplama aracının başkasına ait olup olmadığını belirtin."));

        // ───── 4. Destek ve giderler ─────
        foreach (var d in b.DestekKurumlari.Where(d =>
                     d.KurumTuru is DestekKurumTuru.Universite or DestekKurumTuru.Uluslararasi or DestekKurumTuru.Diger
                     && Bos(d.Aciklama)))
            e.Add(new(4, "DestekKurumlari", $"'{Etiketler.Ad(d.KurumTuru)}' destek kurumunu belirtin."));

        // ───── 5. Proje içeriği ─────
        if (Bos(b.TurkceOzet)) e.Add(new(5, nameof(b.TurkceOzet), "Türkçe özet zorunludur."));
        if (Bos(b.AnahtarKelimeler)) e.Add(new(5, nameof(b.AnahtarKelimeler), "Anahtar kelimeler zorunludur."));
        if (Bos(b.YabanciDilOzet)) e.Add(new(5, nameof(b.YabanciDilOzet), "Yabancı dilde özet zorunludur."));
        if (Bos(b.Keywords)) e.Add(new(5, nameof(b.Keywords), "Keywords zorunludur."));
        if (Bos(b.Amac)) e.Add(new(5, nameof(b.Amac), "Araştırmanın amacı zorunludur."));
        if (Bos(b.KonuKapsamLiteratur)) e.Add(new(5, nameof(b.KonuKapsamLiteratur), "Konu, kapsam ve literatür özeti zorunludur."));
        if (Bos(b.MateryalYontem)) e.Add(new(5, nameof(b.MateryalYontem), "Materyal ve yöntem zorunludur."));
        if (Bos(b.OzgunDeger)) e.Add(new(5, nameof(b.OzgunDeger), "Özgün değer zorunludur."));
        if (Bos(b.YayginEtki)) e.Add(new(5, nameof(b.YayginEtki), "Yaygın etki zorunludur."));

        // ───── 6. İş paketleri ve riskler ─────
        if (b.IsPaketleri.Count == 0)
            e.Add(new(6, "IsPaketleri", "En az bir iş paketi ekleyin."));
        if (b.Riskler.Count == 0)
            e.Add(new(6, "Riskler", "En az bir risk ve B planı ekleyin."));

        // ───── 7. Etik sorular ve katılımcılar ─────
        EvetHayirKontrol(e, b.YanlisBilgiVeyaGizlilikGerekiyorMu, b.YanlisBilgiVeyaGizlilikAciklama, true, "9", nameof(b.YanlisBilgiVeyaGizlilikGerekiyorMu));
        EvetHayirKontrol(e, b.SaglikTehdidiVarMi, b.SaglikTehdidiAciklama, true, "10", nameof(b.SaglikTehdidiVarMi));
        EvetHayirKontrol(e, b.KisilikHaklariKorunuyorMu, b.KisilikHaklariAciklama, true, "11", nameof(b.KisilikHaklariKorunuyorMu));
        EvetHayirKontrol(e, b.ResitOlmayanKatilimciVarMi, b.ResitOlmayanKatilimciAciklama, true, "12", nameof(b.ResitOlmayanKatilimciVarMi));
        EvetHayirKontrol(e, b.YeterliBilgiVeriliyorMu, b.YeterliBilgiAciklama, false, "13", nameof(b.YeterliBilgiVeriliyorMu));
        if (Bos(b.GonulluKatilimOnlemleri)) e.Add(new(7, nameof(b.GonulluKatilimOnlemleri), "Soru 14: Gönüllü katılımı koruyan önlemleri açıklayın."));
        if (Bos(b.KatilmaCikmaKosullari)) e.Add(new(7, nameof(b.KatilmaCikmaKosullari), "Soru 15: Katılma ve çıkma koşullarını açıklayın."));
        if (b.BeklenenKatilimciSayisi is null or <= 0)
            e.Add(new(7, nameof(b.BeklenenKatilimciSayisi), "Soru 16: Beklenen katılımcı sayısını girin."));

        var katilimcilar = Secilenler(b, SecenekGrubu.KatilimciGrubu);
        if (katilimcilar.Count == 0)
            e.Add(new(7, "SecenekIdleri", "Soru 17: Katılımcıları tanımlayan en az bir seçenek işaretleyin."));
        if (katilimcilar.Any(s => s.AciklamaGerekirMi) && Bos(b.KatilimciGrubuDiger))
            e.Add(new(7, nameof(b.KatilimciGrubuDiger), "Soru 17: 'Diğer' katılımcı grubunu belirtin."));
        if (katilimcilar.Any(s => s.CocukRizasiGerektirirMi) && b.ResitOlmayanKatilimciVarMi == false)
            e.Add(new(7, nameof(b.ResitOlmayanKatilimciVarMi),
                "18 yaş altı bir katılımcı grubu seçtiniz; Soru 12'yi 'Evet' olarak cevaplayın."));

        var uygulamalar = Secilenler(b, SecenekGrubu.Uygulama);
        if (uygulamalar.Count == 0)
            e.Add(new(7, "SecenekIdleri", "Soru 18: Çalışmada yer alacak en az bir uygulamayı işaretleyin."));
        if (uygulamalar.Any(s => s.AciklamaGerekirMi) && Bos(b.UygulamaDiger))
            e.Add(new(7, nameof(b.UygulamaDiger), "Soru 18: 'Diğer' uygulamayı belirtin."));

        return e;
    }

    /// <summary>Evet/Hayır sorusu cevaplanmış mı; gerekiyorsa açıklama yazılmış mı?</summary>
    private static void EvetHayirKontrol(List<EksikAlan> e, bool? cevap, string? aciklama, bool aciklamaGerektirenCevap,
        string soruNo, string alan)
    {
        if (cevap is null)
            e.Add(new(7, alan, $"Soru {soruNo}'u Evet/Hayır olarak cevaplayın."));
        else if (cevap == aciklamaGerektirenCevap && Bos(aciklama))
            e.Add(new(7, alan, $"Soru {soruNo}: '{(aciklamaGerektirenCevap ? "Evet" : "Hayır")}' cevabı için açıklama yazın."));
    }

    private static List<Secenek> Secilenler(Basvuru b, SecenekGrubu grup) =>
        b.Secimler.Where(s => s.Secenek?.Grup == grup).Select(s => s.Secenek!).ToList();

    private static bool Bos(string? s) => string.IsNullOrWhiteSpace(s);
}
