using System.ComponentModel.DataAnnotations;
using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Dtos;

// ─────────────────────────────────────────────────────────────
// Başvuru formu ile frontend arasında taşınan veri.
// Taslakta alanların hepsi boş olabilir; eksiksizlik kontrolü
// "Gönder" aşamasında yapılacak.
// ─────────────────────────────────────────────────────────────

public class BasvuruOlusturIstegi
{
    [MaxLength(500)]
    public string? ArastirmaAdi { get; set; }

    public BasvuruTuru BasvuruTuru { get; set; } = BasvuruTuru.YeniBasvuru;

    /// <summary>Revizyon veya devam başvurusuysa önceki başvurunun Id'si</summary>
    public int? UstBasvuruId { get; set; }
}

/// <summary>
/// Wizard'ın her "Kaydet / İleri" tıklamasında formun tamamı bu şekilde gönderilir.
/// Listeler (araştırmacılar, giderler...) gönderilen haliyle değiştirilir.
/// </summary>
public class BasvuruFormuDto
{
    [Range(1, 20)]
    public int SonAdim { get; set; } = 1;

    // Ön kontrol
    public bool? DeneyVeyaBiyolojikMateryalVarMi { get; set; }

    // Genel bilgiler
    [MaxLength(500)] public string? ArastirmaAdi { get; set; }
    public BasvuruTuru BasvuruTuru { get; set; } = BasvuruTuru.YeniBasvuru;
    public ArastirmaNiteligi? ArastirmaNiteligi { get; set; }
    [MaxLength(300)] public string? ArastirmaNiteligiDiger { get; set; }
    [MaxLength(500)] public string? BagliKurum { get; set; }
    public int? UstBasvuruId { get; set; }

    // Veri toplama
    [MaxLength(300)] public string? VeriToplamaAraciDiger { get; set; }
    public DateOnly? VeriToplamaBaslangic { get; set; }
    public DateOnly? VeriToplamaBitis { get; set; }
    [Range(0, 120)] public int? TahminiSureAy { get; set; }
    public bool? VeriAraciBaskasinaAitMi { get; set; }

    // 1–6 Proje içeriği
    public string? TurkceOzet { get; set; }
    [MaxLength(500)] public string? AnahtarKelimeler { get; set; }
    public string? YabanciDilOzet { get; set; }
    [MaxLength(500)] public string? Keywords { get; set; }
    public string? Amac { get; set; }
    public string? KonuKapsamLiteratur { get; set; }
    public string? MateryalYontem { get; set; }
    public string? OzgunDeger { get; set; }
    public string? YayginEtki { get; set; }

    // 9–18 Etik sorular
    public bool? YanlisBilgiVeyaGizlilikGerekiyorMu { get; set; }
    public string? YanlisBilgiVeyaGizlilikAciklama { get; set; }
    public bool? SaglikTehdidiVarMi { get; set; }
    public string? SaglikTehdidiAciklama { get; set; }
    public bool? KisilikHaklariKorunuyorMu { get; set; }
    public string? KisilikHaklariAciklama { get; set; }
    public bool? ResitOlmayanKatilimciVarMi { get; set; }
    public string? ResitOlmayanKatilimciAciklama { get; set; }
    public bool? YeterliBilgiVeriliyorMu { get; set; }
    public string? YeterliBilgiAciklama { get; set; }
    public string? GonulluKatilimOnlemleri { get; set; }
    public string? KatilmaCikmaKosullari { get; set; }
    [Range(0, 1_000_000)] public int? BeklenenKatilimciSayisi { get; set; }
    [MaxLength(300)] public string? KatilimciGrubuDiger { get; set; }
    [MaxLength(300)] public string? UygulamaDiger { get; set; }

    // Çoklu seçimler: işaretlenen Secenek Id'leri (GET /api/tanimlar/secenekler)
    public List<int> SecenekIdleri { get; set; } = [];

    // Tekrarlanan bölümler
    public List<ArastirmaciDto> Arastirmacilar { get; set; } = [];
    public List<VeriToplamaYeriDto> VeriToplamaYerleri { get; set; } = [];
    public List<DestekKurumuDto> DestekKurumlari { get; set; } = [];
    public List<GiderKalemiDto> GiderKalemleri { get; set; } = [];
    public List<IsPaketiDto> IsPaketleri { get; set; } = [];
    public List<RiskDto> Riskler { get; set; } = [];
    public List<LiteraturDto> LiteraturKaynaklari { get; set; } = [];
}

/// <summary>Başvurunun tamamı: formdaki alanlar + sistemin tuttuğu bilgiler.</summary>
public class BasvuruDetayDto : BasvuruFormuDto
{
    public int Id { get; set; }
    public BasvuruDurumu Durum { get; set; }
    public bool FormKilitliMi { get; set; }
    public bool DuzenlenebilirMi { get; set; }
    public string? EvrakKayitNo { get; set; }
    public DateTime? EvrakKayitTarihi { get; set; }
    public decimal ToplamGider { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime GuncellemeTarihi { get; set; }
    public DateTime? GonderimTarihi { get; set; }
}

public record BasvuruOzetDto(
    int Id,
    string ArastirmaAdi,
    BasvuruTuru BasvuruTuru,
    BasvuruDurumu Durum,
    int SonAdim,
    string? EvrakKayitNo,
    DateTime OlusturmaTarihi,
    DateTime GuncellemeTarihi,
    DateTime? GonderimTarihi);

// ───── Tekrarlanan bölümlerin satırları ─────

public class ArastirmaciDto
{
    /// <summary>Yeni satırda 0. Var olan satırı güncellerken Id'si gönderilmeli (özgeçmiş eki bağlı kalsın diye).</summary>
    public int Id { get; set; }
    public ArastirmaciRolu Rol { get; set; } = ArastirmaciRolu.EkipUyesi;
    [MaxLength(100)] public string? Unvan { get; set; }
    [Required(ErrorMessage = "Araştırmacının adı soyadı boş olamaz.")]
    [MaxLength(200)] public string AdSoyad { get; set; } = string.Empty;
    [MaxLength(30)] public string? Telefon { get; set; }
    [MaxLength(256)] [EmailAddress(ErrorMessage = "Araştırmacı e-postası geçersiz.")] public string? Email { get; set; }
    [MaxLength(500)] public string? GorevYeriAdres { get; set; }
}

public class VeriToplamaYeriDto
{
    [Required, MaxLength(300)] public string Ad { get; set; } = string.Empty;
}

public class DestekKurumuDto
{
    public DestekDurumu Durum { get; set; }
    public DestekKurumTuru KurumTuru { get; set; }
    [MaxLength(300)] public string? Aciklama { get; set; }
}

public class GiderKalemiDto
{
    public GiderTuru Tur { get; set; }
    [Required, MaxLength(300)] public string Aciklama { get; set; } = string.Empty;
    [Range(0, 1_000_000_000)] public decimal Tutar { get; set; }
}

public class IsPaketiDto
{
    [Range(1, 100)] public int No { get; set; }
    [Required, MaxLength(300)] public string Ad { get; set; } = string.Empty;
    [MaxLength(300)] public string? Kimler { get; set; }
    [MaxLength(100)] public string? Sure { get; set; }
}

public class RiskDto
{
    [Range(1, 100)] public int IsPaketiNo { get; set; }
    [Required, MaxLength(1000)] public string RiskAciklamasi { get; set; } = string.Empty;
    [MaxLength(1000)] public string? BPlani { get; set; }
}

public class LiteraturDto
{
    public LiteraturTuru Tur { get; set; }
    [Required, MaxLength(1000)] public string Kaynak { get; set; } = string.Empty;
}

// ───── Gerekli ekler ─────

public record GerekliEkDto(
    int EkTuruId,
    string Kod,
    string Ad,
    string? Aciklama,
    bool ZorunluMu,
    string Neden,
    int GerekenAdet,
    int YuklenenAdet,
    bool TamamMi,
    bool SistemUretirMi,
    bool CokluDosyaMi);

// ───── Tanımlar ─────

public record SecenekDto(int Id, SecenekGrubu Grup, string Ad, int Sira, bool AciklamaGerekirMi, bool CocukRizasiGerektirirMi);

public record EkTuruDto(int Id, string Kod, string Ad, string? Aciklama, EkZorunlulukKurali ZorunlulukKurali, bool CokluDosyaMi, bool SistemUretirMi);
