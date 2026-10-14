namespace EtikKurul.Api.Entities;

/// <summary>
/// Çoklu seçim kutularındaki sabit seçenekler (Anket, Görüşme, Lise Öğrencileri ...).
/// Ayrı tablo olmaları raporlamayı kolaylaştırır: "Kaç başvuruda çocuk katılımcı var?"
/// Başlangıç verileri: Data/SabitVeriler.cs
/// </summary>
public class Secenek
{
    public int Id { get; set; }
    public SecenekGrubu Grup { get; set; }
    public string Ad { get; set; } = string.Empty;
    public int Sira { get; set; }

    /// <summary>"Diğer (belirtiniz)" gibi seçilince açıklama isteyen seçenek mi?</summary>
    public bool AciklamaGerekirMi { get; set; }

    /// <summary>Seçilirse Ek-3 Çocuk Rıza Formu zorunlu olur (18 yaş altı gruplar).</summary>
    public bool CocukRizasiGerektirirMi { get; set; }

    public bool AktifMi { get; set; } = true;
}

/// <summary>Başvurunun işaretlediği seçenekler (Basvuru ↔ Secenek, çoka-çok ara tablo).</summary>
public class BasvuruSecenek
{
    public int BasvuruId { get; set; }
    public Basvuru? Basvuru { get; set; }

    public int SecenekId { get; set; }
    public Secenek? Secenek { get; set; }
}
