namespace EtikKurul.Api.Constants;

/// <summary>
/// Sistemdeki roller. Rol adlarını kodun her yerinde buradan kullanın,
/// elle "Admin" gibi yazmayın; yazım hatası yaparsanız derleyici yakalar.
/// </summary>
public static class Roller
{
    public const string Basvuran = "Basvuran";
    public const string EvrakKayit = "EvrakKayit";
    public const string Dekanlik = "Dekanlik";
    public const string EtikKurul = "EtikKurul";
    public const string Admin = "Admin";

    public static readonly string[] Tumu = [Basvuran, EvrakKayit, Dekanlik, EtikKurul, Admin];
}
