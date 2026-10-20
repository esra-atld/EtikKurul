namespace EtikKurul.Api.Entities;

/// <summary>
/// Etik kurulun başvuru hakkındaki kararı. Her başvurunun en fazla bir kararı olur.
/// Karar, 3 üyenin oyları tamamlanınca sistem tarafından oluşturulur:
///   kabul oyu çoksa → Uygun (kabul), red oyu çoksa → UygunDegil (red)
///   oyların hepsi aynıysa → oy birliği, değilse → oy çokluğu
/// </summary>
public class Karar
{
    public int Id { get; set; }

    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    /// <summary>Kurulun karar numarası (ileride verilecekse). Şimdilik boş kalabilir.</summary>
    public string? KararNo { get; set; }
    public DateTime KararTarihi { get; set; }
    public KararTuru KararTuru { get; set; }

    /// <summary>true: oy birliği, false: oy çokluğu</summary>
    public bool OyBirligiMi { get; set; }
    public int KabulOySayisi { get; set; }
    public int RedOySayisi { get; set; }

    /// <summary>Gerekçe veya açıklama</summary>
    public string? Aciklama { get; set; }

    /// <summary>Kararı kaydeden kişi. Oylamayla oluşan kararda boştur (sistem oluşturur).</summary>
    public string? KararVerenKullaniciId { get; set; }
    public Kullanici? KararVerenKullanici { get; set; }

    /// <summary>
    /// QR koddaki benzersiz, tahmin edilemez kod.
    /// QR → https://site/dogrula/{DogrulamaKodu} → "Bu belge geçerlidir".
    /// </summary>
    public Guid DogrulamaKodu { get; set; } = Guid.NewGuid();

    /// <summary>Üretilen karar PDF'inin diskteki adı</summary>
    public string? KararBelgesiDosyaAdi { get; set; }

    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
}
