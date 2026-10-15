using System.ComponentModel.DataAnnotations;
using EtikKurul.Api.Services;

namespace EtikKurul.Api.Dtos;

/// <summary>Ek yükleme isteği (multipart/form-data).</summary>
public class EkYuklemeIstegi
{
    /// <summary>Hangi belge? (GET /api/basvurular/{id}/gerekli-ekler → ekTuruId)</summary>
    [Required]
    public int EkTuruId { get; set; }

    /// <summary>Sadece Ek-7 özgeçmiş için: kime ait? (başvurudaki araştırmacının id'si)</summary>
    public int? BasvuruArastirmacisiId { get; set; }

    /// <summary>Islak imzalı, taranmış PDF</summary>
    [Required]
    public IFormFile Dosya { get; set; } = null!;
}

public record EkDto(
    int Id,
    int EkTuruId,
    string EkTuruKod,
    string EkTuruAd,
    int? BasvuruArastirmacisiId,
    string OrijinalDosyaAdi,
    long BoyutByte,
    DateTime YuklemeTarihi);

public record EksikAlanlarCevabi(bool TamamMi, List<EksikAlan> Eksikler);
