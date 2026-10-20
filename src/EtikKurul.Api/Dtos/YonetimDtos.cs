using System.ComponentModel.DataAnnotations;
using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Dtos;

// Yetkililerin (Arşiv Kayıt, Yetkili Birim, Etik Kurul) ekranları için veri şekilleri.

/// <summary>Görev listelerindeki bir satır</summary>
public record YonetimOzetDto(
    int Id,
    string ArastirmaAdi,
    string BasvuranAdSoyad,
    string BasvuranBirim,
    BasvuruDurumu Durum,
    string? EvrakKayitNo,
    DateTime? EvrakKayitTarihi,
    DateTime? GonderimTarihi,
    string? Sonuc,
    bool? BenimOyum); // Etik kurul üyesi için: kendi oyu (null: henüz vermedi)

/// <summary>Başvurunun durum geçmişindeki bir satır</summary>
public record HareketDto(
    DateTime Tarih,
    BasvuruDurumu? OncekiDurum,
    BasvuruDurumu YeniDurum,
    string? Aciklama,
    string? IslemYapan); // İşlemi yapan kişi (başvurana gösterilmez)

public record KararOzetDto(
    KararTuru KararTuru,
    bool OyBirligiMi,
    int KabulOySayisi,
    int RedOySayisi,
    DateTime KararTarihi,
    string Ozet);

public record OylamaDurumuDto(
    int UyeSayisi,
    int OyVerenSayisi,
    bool? BenimOyum, // Giriş yapan üyenin oyu (üye değilse veya vermediyse null)
    bool BenimOyumBekleniyor);

public record BasvuranDto(
    string AdSoyad,
    string CalistigiBirim,
    string Email,
    string KurumsalEmail,
    string? Telefon);

/// <summary>Yetkilinin başvuru inceleme sayfası</summary>
public record YonetimDetayDto(
    BasvuruDetayDto Basvuru,
    BasvuranDto Basvuran,
    List<HareketDto> Hareketler,
    List<EkDto> Ekler,
    OylamaDurumuDto? Oylama,
    KararOzetDto? Karar,
    // Giriş yapan kişinin bu başvuruda yapabileceği işlemler:
    // "ArsiveKaydet", "EtikKuruluGonder", "OyVer", "BasvuranaIlet"
    List<string> Islemler);

public class ArsiveKaydetIstegi
{
    [Required(ErrorMessage = "Kayıt sayısı zorunludur.")]
    [MaxLength(50)]
    public string KayitNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kayıt tarihi zorunludur.")]
    public DateTime KayitTarihi { get; set; }
}

public class OyIstegi
{
    /// <summary>true: onaylıyorum, false: onaylamıyorum</summary>
    [Required]
    public bool? Onayliyorum { get; set; }
}

/// <summary>Başvuranın "başvurum nerede?" ekranı</summary>
public record TakipDto(
    BasvuruDurumu Durum,
    string? EvrakKayitNo,
    DateTime? EvrakKayitTarihi,
    List<HareketDto> Hareketler,
    KararOzetDto? Karar);
