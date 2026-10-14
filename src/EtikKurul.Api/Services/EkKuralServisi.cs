using EtikKurul.Api.Dtos;
using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Services;

/// <summary>
/// "Ek sayısı yapacağın işe göre değişir": başvurunun cevaplarına bakıp
/// hangi eklerin zorunlu olduğunu ve kaçının yüklendiğini hesaplar.
/// Kurallar EkTurleri tablosundaki ZorunlulukKurali alanından gelir.
/// </summary>
public static class EkKuralServisi
{
    // Ek-10'u (veri toplama araçları) gerektiren seçenekler: anket, görüşme, ölçek, mülakat, bilgisayarda test
    private static readonly HashSet<int> VeriToplamaAraciGerektirenSecenekler = [1, 2, 4, 201, 202, 204, 207];

    private static readonly HashSet<ArastirmaNiteligi> TezNitelikleri =
    [
        ArastirmaNiteligi.YuksekLisansTezi,
        ArastirmaNiteligi.DoktoraSanattaYeterlilikTezi,
        ArastirmaNiteligi.TezsizYuksekLisansProjesi,
    ];

    /// <param name="b">Arastirmacilar, Secimler(+Secenek), DestekKurumlari ve Ekler yüklenmiş olmalı.</param>
    public static List<GerekliEkDto> Hesapla(Basvuru b, IEnumerable<EkTuru> ekTurleri)
    {
        var sonuc = new List<GerekliEkDto>();

        foreach (var tur in ekTurleri.Where(t => t.AktifMi).OrderBy(t => t.Sira))
        {
            var (zorunlu, neden) = KuraliDegerlendir(tur.ZorunlulukKurali, b);

            // Gerekmeyen ve isteğe bağlı da olmayan ekleri listeye koyma
            if (!zorunlu && tur.ZorunlulukKurali != EkZorunlulukKurali.Opsiyonel)
                continue;

            var yuklenenler = b.Ekler.Where(e => e.EkTuruId == tur.Id).ToList();
            int gereken, yuklenen;
            bool tamam;

            if (tur.ZorunlulukKurali == EkZorunlulukKurali.HerArastirmaciIcin)
            {
                // Her araştırmacı için ayrı özgeçmiş
                gereken = b.Arastirmacilar.Count;
                yuklenen = b.Arastirmacilar.Count(a => yuklenenler.Any(e => e.BasvuruArastirmacisiId == a.Id));
                tamam = yuklenen >= gereken;
            }
            else
            {
                gereken = zorunlu ? 1 : 0;
                yuklenen = yuklenenler.Count;
                tamam = !zorunlu || yuklenen > 0;
            }

            sonuc.Add(new GerekliEkDto(
                tur.Id, tur.Kod, tur.Ad, tur.Aciklama, zorunlu, neden,
                gereken, yuklenen, tamam, tur.SistemUretirMi, tur.CokluDosyaMi));
        }

        return sonuc;
    }

    private static (bool Zorunlu, string Neden) KuraliDegerlendir(EkZorunlulukKurali kural, Basvuru b) => kural switch
    {
        EkZorunlulukKurali.HerZaman =>
            (true, "Her başvuruda zorunludur."),

        EkZorunlulukKurali.Opsiyonel =>
            (false, "İsteğe bağlıdır."),

        EkZorunlulukKurali.ResitOlmayanKatilimciVarsa =>
            b.ResitOlmayanKatilimciVarMi == true || b.Secimler.Any(s => s.Secenek?.CocukRizasiGerektirirMi == true)
                ? (true, "Araştırmaya 18 yaş altı katılımcılar katılıyor.")
                : (false, ""),

        EkZorunlulukKurali.DestekleyenKurumVarsa =>
            b.DestekKurumlari.Any(d => d.Durum == DestekDurumu.Destekleyen)
                ? (true, "Araştırmayı destekleyen bir kurum belirttiniz.")
                : (false, ""),

        EkZorunlulukKurali.TezCalismasiysa =>
            b.ArastirmaNiteligi is { } n && TezNitelikleri.Contains(n)
                ? (true, "Araştırma bir tez çalışması.")
                : (false, ""),

        EkZorunlulukKurali.HerArastirmaciIcin =>
            b.Arastirmacilar.Count > 0
                ? (true, "Her araştırmacı için bir özgeçmiş yüklenmelidir.")
                : (false, ""),

        EkZorunlulukKurali.ProjeVeyaTezIse =>
            b.ArastirmaNiteligi is { } m && m != ArastirmaNiteligi.Diger
                ? (true, "BAP projesi veya tez çalışması.")
                : (false, ""),

        EkZorunlulukKurali.VeriToplamaAraciVarsa =>
            b.Secimler.Any(s => VeriToplamaAraciGerektirenSecenekler.Contains(s.SecenekId))
                ? (true, "Anket, ölçek, görüşme gibi bir veri toplama aracı kullanılıyor.")
                : (false, ""),

        EkZorunlulukKurali.VeriAraciBaskasinaAitse =>
            b.VeriAraciBaskasinaAitMi == true
                ? (true, "Kullanılacak veri toplama aracı başkasına ait.")
                : (false, ""),

        _ => (false, ""),
    };
}
