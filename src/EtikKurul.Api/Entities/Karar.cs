namespace EtikKurul.Api.Entities;

/// <summary>
/// Etik kurulun başvuru hakkındaki kararı. Her başvurunun en fazla bir kararı olur.
/// (Düzeltme istenirse başvuran yeni bir "Tekrar Başvuru (revizyon)" açar.)
/// </summary>
public class Karar
{
    public int Id { get; set; }

    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    /// <summary>Ör. "2026/15". Kurulun karar defterindeki numara.</summary>
    public string KararNo { get; set; } = string.Empty;
    public DateTime KararTarihi { get; set; }
    public KararTuru KararTuru { get; set; }

    /// <summary>Gerekçe veya istenen düzeltmeler</summary>
    public string? Aciklama { get; set; }

    public string KararVerenKullaniciId { get; set; } = string.Empty;
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
