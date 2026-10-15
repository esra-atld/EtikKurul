namespace EtikKurul.Api.Services;

/// <summary>
/// Yüklenen PDF'leri diske kaydeder, okur ve siler.
/// Dosyalar tahmin edilemez adlarla (GUID) saklanır; veritabanında sadece bu ad tutulur.
/// Klasör: appsettings → "Dosyalar:KlasorYolu" (varsayılan: App_Data/Yuklemeler).
/// Bu klasör .gitignore'da; yüklenen belgeler GitHub'a GİTMEZ.
/// </summary>
public class DosyaDepolamaServisi
{
    private static readonly byte[] PdfImzasi = "%PDF-"u8.ToArray();

    private readonly string _klasor;
    public long MaksimumBoyutByte { get; }

    public DosyaDepolamaServisi(IConfiguration ayarlar, IWebHostEnvironment ortam)
    {
        var yol = ayarlar["Dosyalar:KlasorYolu"] ?? "App_Data/Yuklemeler";
        _klasor = Path.IsPathRooted(yol) ? yol : Path.Combine(ortam.ContentRootPath, yol);
        Directory.CreateDirectory(_klasor);

        var mb = ayarlar.GetValue<int?>("Dosyalar:MaksimumBoyutMB") ?? 10;
        MaksimumBoyutByte = mb * 1024L * 1024L;
    }

    /// <summary>Dosya gerçekten PDF mi? Uzantıya değil, dosyanın ilk baytlarına bakar.</summary>
    public static async Task<bool> PdfMiAsync(IFormFile dosya)
    {
        if (!Path.GetExtension(dosya.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return false;

        var baslik = new byte[PdfImzasi.Length];
        await using var akis = dosya.OpenReadStream();
        var okunan = await akis.ReadAtLeastAsync(baslik, baslik.Length, throwOnEndOfStream: false);
        return okunan == PdfImzasi.Length && baslik.AsSpan().SequenceEqual(PdfImzasi);
    }

    /// <summary>Dosyayı kaydeder ve diskteki yeni adını döner (ör. "3f2a...e1.pdf").</summary>
    public async Task<string> KaydetAsync(IFormFile dosya)
    {
        var ad = $"{Guid.NewGuid():N}.pdf";
        await using var hedef = File.Create(TamYol(ad));
        await dosya.CopyToAsync(hedef);
        return ad;
    }

    public string TamYol(string kayitliAd)
    {
        // Güvenlik: "../../" gibi adlarla klasör dışına çıkılamasın
        var temizAd = Path.GetFileName(kayitliAd);
        return Path.Combine(_klasor, temizAd);
    }

    public bool VarMi(string kayitliAd) => File.Exists(TamYol(kayitliAd));

    public void Sil(string kayitliAd)
    {
        var yol = TamYol(kayitliAd);
        if (File.Exists(yol))
            File.Delete(yol);
    }
}
