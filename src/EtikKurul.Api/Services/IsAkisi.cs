using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Services;

/// <summary>
/// Başvurunun durumunu değiştirmenin TEK yolu.
/// Durum her değiştiğinde hareket tablosuna da satır eklenir; böylece
/// "evrak nerede, kim ne zaman ne yaptı?" sorusunun cevabı hiç kaybolmaz.
/// Durumu asla doğrudan "basvuru.Durum = ..." diye değiştirmeyin, bunu kullanın.
/// </summary>
public static class IsAkisi
{
    public static void DurumDegistir(Basvuru basvuru, BasvuruDurumu yeniDurum, string kullaniciId, string? aciklama = null)
    {
        var simdi = DateTime.UtcNow;

        basvuru.Hareketler.Add(new BasvuruHareketi
        {
            BasvuruId = basvuru.Id,
            OncekiDurum = basvuru.Durum,
            YeniDurum = yeniDurum,
            IslemYapanKullaniciId = kullaniciId,
            Tarih = simdi,
            Aciklama = aciklama,
        });

        basvuru.Durum = yeniDurum;
        basvuru.GuncellemeTarihi = simdi;
    }
}
