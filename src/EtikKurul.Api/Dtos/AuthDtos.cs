using System.ComponentModel.DataAnnotations;

namespace EtikKurul.Api.Dtos;

// DTO = Data Transfer Object: frontend ile API arasında taşınan veri.
// Entity'yi (veritabanı sınıfını) doğrudan dışarı açmıyoruz; şifre hash'i gibi
// alanlar sızmasın ve frontend sadece ihtiyacı olanı görsün diye.

public class KayitIstegi
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [MaxLength(100)]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [MaxLength(100)]
    public string Soyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Çalıştığı birim zorunludur.")]
    [MaxLength(200)]
    public string CalistigiBirim { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kurumsal e-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir kurumsal e-posta girin.")]
    public string KurumsalEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [RegularExpression(@"^0\d{10}$", ErrorMessage = "Telefon numarası 0 ile başlayan 11 haneli olmalı (ör. 05551234567).")]
    public string Telefon { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur.")]
    [MaxLength(500)]
    public string Adres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Sifre { get; set; } = string.Empty;
}

public class GirisIstegi
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Sifre { get; set; } = string.Empty;
}

public record GirisCevabi(string Token, DateTime BitisZamani, KullaniciDto Kullanici);

public record KullaniciDto(
    string Id,
    string Ad,
    string Soyad,
    string Email,
    string KurumsalEmail,
    string? Telefon,
    string CalistigiBirim,
    string Adres,
    bool AktifMi,
    DateTime KayitTarihi,
    IList<string> Roller);

public class RolGuncellemeIstegi
{
    [Required]
    public List<string> Roller { get; set; } = [];
}

public class AktiflikIstegi
{
    public bool AktifMi { get; set; }
}
