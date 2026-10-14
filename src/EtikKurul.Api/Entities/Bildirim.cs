namespace EtikKurul.Api.Entities;

/// <summary>
/// Gönderilen e-postaların kaydı ("belge yüklendi → etik kurula mail" gibi).
/// Mail gitmediyse burada BasariliMi = false ve hata mesajı görünür.
/// </summary>
public class Bildirim
{
    public int Id { get; set; }

    public int? BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public string? AliciKullaniciId { get; set; }
    public Kullanici? AliciKullanici { get; set; }

    public string AliciEmail { get; set; } = string.Empty;
    public string Konu { get; set; } = string.Empty;
    public string Icerik { get; set; } = string.Empty;

    public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? GonderimTarihi { get; set; }
    public bool BasariliMi { get; set; }
    public string? HataMesaji { get; set; }
}
