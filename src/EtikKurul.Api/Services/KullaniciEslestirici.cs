using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Services;

/// <summary>Kullanici (veritabanı sınıfı) → KullaniciDto (dışarıya giden veri) dönüşümü.</summary>
public static class KullaniciEslestirici
{
    public static KullaniciDto DtoyaCevir(Kullanici k, IList<string> roller) => new(
        k.Id,
        k.Ad,
        k.Soyad,
        k.Email ?? string.Empty,
        k.KurumsalEmail,
        k.PhoneNumber,
        k.CalistigiBirim,
        k.Adres,
        k.AktifMi,
        k.KayitTarihi,
        roller);
}
