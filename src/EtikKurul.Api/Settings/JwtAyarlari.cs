namespace EtikKurul.Api.Settings;

/// <summary>appsettings içindeki "Jwt" bölümü bu sınıfa okunur.</summary>
public class JwtAyarlari
{
    /// <summary>Token'ı imzalayan gizli anahtar. En az 32 karakter olmalı. GitHub'a gerçek anahtarı koymayın.</summary>
    public string Anahtar { get; set; } = string.Empty;
    public string Yayinci { get; set; } = string.Empty;
    public string Hedef { get; set; } = string.Empty;
    public int SureDakika { get; set; } = 120;
}
