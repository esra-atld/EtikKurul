namespace EtikKurul.Api.Entities;

/// <summary>
/// Etik kurul başvurusu: sistemin ana tablosu.
/// Formdaki tek değerli alanlar burada, tekrarlanan tablolar (araştırmacılar, giderler...) ayrı tablolarda.
/// Bölüm numaraları "TF Etik Kurul Başvuru Formu"ndaki numaralardır.
/// </summary>
public class Basvuru
{
    public int Id { get; set; }

    // ───── Sahiplik ve iş akışı ─────
    public string BasvuranKullaniciId { get; set; } = string.Empty;
    public Kullanici? BasvuranKullanici { get; set; }

    public BasvuruDurumu Durum { get; set; } = BasvuruDurumu.Taslak;

    /// <summary>Wizard'da en son kaydedilen adım. "Kaldığı yerden devam" için.</summary>
    public int SonAdim { get; set; } = 1;

    /// <summary>PDF çıktısı alındıktan sonra true olur; form artık değiştirilemez.</summary>
    public bool FormKilitliMi { get; set; }

    /// <summary>Revizyon / devam başvurusuysa önceki başvurunun Id'si.</summary>
    public int? UstBasvuruId { get; set; }
    public Basvuru? UstBasvuru { get; set; }

    // ───── Evrak kayıt ─────
    /// <summary>Evrak Kayıt'ın verdiği numara (ör. 2026/0042). Kayda alınınca dolar.</summary>
    public string? EvrakKayitNo { get; set; }
    public DateTime? EvrakKayitTarihi { get; set; }

    // ───── Ön kontrol (formun başındaki uyarı) ─────
    /// <summary>İnsan/hayvan üzerinde deney veya biyolojik materyal varsa bu kurul değil, sağlık/klinik/hayvan kurulu.</summary>
    public bool? DeneyVeyaBiyolojikMateryalVarMi { get; set; }

    // ───── Genel bilgiler ─────
    public string ArastirmaAdi { get; set; } = string.Empty;
    public BasvuruTuru BasvuruTuru { get; set; } = BasvuruTuru.YeniBasvuru;
    public ArastirmaNiteligi? ArastirmaNiteligi { get; set; }
    public string? ArastirmaNiteligiDiger { get; set; }
    public string? BagliKurum { get; set; }

    // ───── Veri toplama ─────
    public string? VeriToplamaAraciDiger { get; set; }
    public DateOnly? VeriToplamaBaslangic { get; set; }
    public DateOnly? VeriToplamaBitis { get; set; }
    public int? TahminiSureAy { get; set; }
    /// <summary>Ek-11 için: kullanılacak anket/ölçek başkasına mı ait?</summary>
    public bool? VeriAraciBaskasinaAitMi { get; set; }

    // ───── 1–6: Proje içeriği ─────
    public string? TurkceOzet { get; set; }
    public string? AnahtarKelimeler { get; set; }
    public string? YabanciDilOzet { get; set; }
    public string? Keywords { get; set; }
    public string? Amac { get; set; }
    public string? KonuKapsamLiteratur { get; set; }
    public string? MateryalYontem { get; set; }
    public string? OzgunDeger { get; set; }
    public string? YayginEtki { get; set; }

    // ───── 9–16: Etik sorular ─────
    public bool? YanlisBilgiVeyaGizlilikGerekiyorMu { get; set; }   // 9
    public string? YanlisBilgiVeyaGizlilikAciklama { get; set; }
    public bool? SaglikTehdidiVarMi { get; set; }                   // 10
    public string? SaglikTehdidiAciklama { get; set; }
    public bool? KisilikHaklariKorunuyorMu { get; set; }            // 11
    public string? KisilikHaklariAciklama { get; set; }
    public bool? ResitOlmayanKatilimciVarMi { get; set; }           // 12
    public string? ResitOlmayanKatilimciAciklama { get; set; }
    public bool? YeterliBilgiVeriliyorMu { get; set; }              // 13 (HAYIR ise açıklama)
    public string? YeterliBilgiAciklama { get; set; }
    public string? GonulluKatilimOnlemleri { get; set; }            // 14
    public string? KatilmaCikmaKosullari { get; set; }              // 15
    public int? BeklenenKatilimciSayisi { get; set; }               // 16
    public string? KatilimciGrubuDiger { get; set; }                // 17 "Diğer"
    public string? UygulamaDiger { get; set; }                      // 18 "Diğer"

    // ───── Zaman damgaları ─────
    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime GuncellemeTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GonderimTarihi { get; set; }

    /// <summary>İki kişi aynı başvuruyu aynı anda değiştirirse ikincisi hata alır (eşzamanlılık kontrolü).</summary>
    public byte[] SatirVersiyonu { get; set; } = [];

    // ───── İlişkili tablolar ─────
    public List<BasvuruArastirmacisi> Arastirmacilar { get; set; } = [];
    public List<VeriToplamaYeri> VeriToplamaYerleri { get; set; } = [];
    public List<BasvuruDestekKurumu> DestekKurumlari { get; set; } = [];
    public List<GiderKalemi> GiderKalemleri { get; set; } = [];
    public List<IsPaketi> IsPaketleri { get; set; } = [];
    public List<Risk> Riskler { get; set; } = [];
    public List<LiteraturKaynagi> LiteraturKaynaklari { get; set; } = [];
    public List<BasvuruSecenek> Secimler { get; set; } = [];
    public List<Ek> Ekler { get; set; } = [];
    public List<BasvuruHareketi> Hareketler { get; set; } = [];
    public List<EtikKurulOyu> Oylar { get; set; } = [];
    public Karar? Karar { get; set; }
}
