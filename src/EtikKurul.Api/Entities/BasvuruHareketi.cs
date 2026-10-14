namespace EtikKurul.Api.Entities;

/// <summary>
/// Başvurunun durum geçmişi. Her durum değişikliğinde bir satır eklenir, ASLA silinmez/güncellenmez.
/// Başvuranın "evrağım nerede?" ekranındaki zaman çizelgesi bu tablodan oluşur.
/// </summary>
public class BasvuruHareketi
{
    public int Id { get; set; }

    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    /// <summary>İlk kayıtta (başvuru oluşturulurken) boştur.</summary>
    public BasvuruDurumu? OncekiDurum { get; set; }
    public BasvuruDurumu YeniDurum { get; set; }

    public string IslemYapanKullaniciId { get; set; } = string.Empty;
    public Kullanici? IslemYapanKullanici { get; set; }

    public DateTime Tarih { get; set; } = DateTime.UtcNow;

    /// <summary>Ör. "Eksik belge: Ek-6 yüklenmemiş" veya "Kurul gündemine alındı"</summary>
    public string? Aciklama { get; set; }
}
