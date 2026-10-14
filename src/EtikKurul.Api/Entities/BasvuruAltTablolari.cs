namespace EtikKurul.Api.Entities;

// Formdaki "satırlar çoğaltılabilir" bölümlerin her biri ayrı tablo (Basvuru ile 1-N ilişki).

/// <summary>Sorumlu araştırmacı ve ekip. Sistem kullanıcısı olmak zorunda değiller.</summary>
public class BasvuruArastirmacisi
{
    public int Id { get; set; }
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public ArastirmaciRolu Rol { get; set; }
    public string? Unvan { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? GorevYeriAdres { get; set; }
    public int Sira { get; set; }
}

/// <summary>"Veri Toplanması Planlanan Yerler/Mekanlar, Kurum ve Kuruluşlar"</summary>
public class VeriToplamaYeri
{
    public int Id { get; set; }
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public string Ad { get; set; } = string.Empty;
    public int Sira { get; set; }
}

/// <summary>Destekleyen / desteklemesi planlanan kurumlar. Hiç satır yoksa = "Yoktur".</summary>
public class BasvuruDestekKurumu
{
    public int Id { get; set; }
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public DestekDurumu Durum { get; set; }
    public DestekKurumTuru KurumTuru { get; set; }
    /// <summary>"Üniversite (belirtiniz)", "Diğer (belirtiniz)" gibi açıklamalar</summary>
    public string? Aciklama { get; set; }
}

/// <summary>"Araştırma Giderleri": hizmet, demirbaş, sarf, diğer. Toplam bu satırlardan hesaplanır.</summary>
public class GiderKalemi
{
    public int Id { get; set; }
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public GiderTuru Tur { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public decimal Tutar { get; set; }
    public int Sira { get; set; }
}

/// <summary>7. Yönetim Düzeni: iş paketleri, görev dağılımı ve süreleri</summary>
public class IsPaketi
{
    public int Id { get; set; }
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public int No { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Kimler { get; set; }
    /// <summary>Ör. "1–3. aylar"</summary>
    public string? Sure { get; set; }
}

/// <summary>8. Risk Yönetimi</summary>
public class Risk
{
    public int Id { get; set; }
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public int IsPaketiNo { get; set; }
    public string RiskAciklamasi { get; set; } = string.Empty;
    public string? BPlani { get; set; }
}

/// <summary>"Seçilmiş Literatür Listesi" ve "Yöntemlerde Yararlanılan Literatür Listesi"</summary>
public class LiteraturKaynagi
{
    public int Id { get; set; }
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public LiteraturTuru Tur { get; set; }
    public string Kaynak { get; set; } = string.Empty;
    public int Sira { get; set; }
}
