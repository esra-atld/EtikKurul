namespace EtikKurul.Api.Entities;

/// <summary>
/// Bir etik kurul üyesinin bir başvuru için verdiği oy.
/// Yetkili birim başvuruyu etik kurula gönderdiğinde, o anki 3 üyenin her biri için
/// boş (Onayladi = null) bir satır açılır. Üye oy verince doldurulur.
/// 3 oy da verilince sistem kararı hesaplar (bkz. Karar).
/// </summary>
public class EtikKurulOyu
{
    public int Id { get; set; }

    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public string UyeKullaniciId { get; set; } = string.Empty;
    public Kullanici? UyeKullanici { get; set; }

    /// <summary>null: henüz oy vermedi, true: onaylıyor, false: onaylamıyor</summary>
    public bool? Onayladi { get; set; }

    /// <summary>Başvurunun üyeye gönderildiği zaman</summary>
    public DateTime GonderimTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? OyTarihi { get; set; }
}
