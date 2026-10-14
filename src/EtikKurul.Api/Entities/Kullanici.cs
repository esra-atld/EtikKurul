using Microsoft.AspNetCore.Identity;

namespace EtikKurul.Api.Entities;

/// <summary>
/// Sisteme kayıtlı kullanıcı.
/// IdentityUser'dan miras aldığı için Id, Email, PasswordHash, PhoneNumber
/// gibi alanlar hazır geliyor. Burada sadece hocanın istediği ek alanlar var.
/// </summary>
public class Kullanici : IdentityUser
{
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;

    /// <summary>Çalıştığı yer / birim (ör. Bilgisayar Mühendisliği Bölümü)</summary>
    public string CalistigiBirim { get; set; } = string.Empty;

    /// <summary>Kurumsal e-posta (ör. ad.soyad@selcuk.edu.tr). Kişisel e-posta Email alanında.</summary>
    public string KurumsalEmail { get; set; } = string.Empty;

    public string Adres { get; set; } = string.Empty;

    /// <summary>Admin bir kullanıcıyı pasif yaparsa giriş yapamaz.</summary>
    public bool AktifMi { get; set; } = true;

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
}
