namespace EtikKurul.Api.Entities;

/// <summary>
/// Belge türleri: Dilekçe, Başvuru Formu, Ek-1 ... Ek-11.
/// Hangi ekin ne zaman zorunlu olduğu ZorunlulukKurali ile belirtilir.
/// Başlangıç verileri: Data/SabitVeriler.cs
/// </summary>
public class EkTuru
{
    public int Id { get; set; }

    /// <summary>Kodda kullanılan sabit kod (ör. "EK1", "DILEKCE"). Değiştirmeyin.</summary>
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string? Aciklama { get; set; }

    public EkZorunlulukKurali ZorunlulukKurali { get; set; }

    /// <summary>Aynı türden birden çok dosya yüklenebilir mi? (ör. her araştırmacı için özgeçmiş)</summary>
    public bool CokluDosyaMi { get; set; }

    /// <summary>Sistem formdaki bilgilerle doldurulmuş PDF'ini üretiyor mu? (dilekçe, taahhütname...)</summary>
    public bool SistemUretirMi { get; set; }

    public int Sira { get; set; }
    public bool AktifMi { get; set; } = true;
}

/// <summary>
/// Yüklenen dosya (ıslak imzalı, taranmış PDF).
/// Dosyanın kendisi diskte durur; veritabanında sadece bilgisi tutulur.
/// </summary>
public class Ek
{
    public int Id { get; set; }

    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public int EkTuruId { get; set; }
    public EkTuru? EkTuru { get; set; }

    /// <summary>Özgeçmiş (Ek-7) gibi kişiye bağlı eklerde hangi araştırmacıya ait olduğu.</summary>
    public int? BasvuruArastirmacisiId { get; set; }
    public BasvuruArastirmacisi? BasvuruArastirmacisi { get; set; }

    /// <summary>Kullanıcının yüklediği dosyanın adı (ör. taahhutname_imzali.pdf)</summary>
    public string OrijinalDosyaAdi { get; set; } = string.Empty;

    /// <summary>Diskte kaydedilen tahmin edilemez ad (ör. 3f2a...e1.pdf). Dosya linki tahmin edilerek açılamasın.</summary>
    public string KayitliDosyaAdi { get; set; } = string.Empty;

    public long BoyutByte { get; set; }
    public string IcerikTuru { get; set; } = "application/pdf";

    public string YukleyenKullaniciId { get; set; } = string.Empty;
    public Kullanici? YukleyenKullanici { get; set; }

    public DateTime YuklemeTarihi { get; set; } = DateTime.UtcNow;
}
