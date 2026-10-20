using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Services;

/// <summary>Enum değerlerinin kullanıcıya gösterilecek Türkçe karşılıkları (PDF, e-posta).</summary>
public static class Etiketler
{
    public static string Ad(BasvuruDurumu d) => d switch
    {
        BasvuruDurumu.Taslak => "Taslak",
        BasvuruDurumu.ImzaliBelgeBekleniyor => "İmzalı belgeler bekleniyor",
        BasvuruDurumu.EvrakKayitta => "Arşiv kayıtta",
        BasvuruDurumu.IadeEdildi => "Başvurana iade edildi",
        BasvuruDurumu.Dekanlikta => "Yetkili birimde",
        BasvuruDurumu.EtikKurulda => "Etik kurulda (oylamada)",
        BasvuruDurumu.KararDekanlikta => "Karar yetkili birimde",
        BasvuruDurumu.Sonuclandi => "Sonuçlandı",
        BasvuruDurumu.Arsivlendi => "Arşivlendi",
        _ => d.ToString(),
    };

    public static string Ad(BasvuruTuru t) => t switch
    {
        BasvuruTuru.YeniBasvuru => "Yeni Başvuru",
        BasvuruTuru.TekrarBasvuruRevizyon => "Tekrar Başvuru (revizyon)",
        BasvuruTuru.ProtokolEkipDegisikligi => "Protokol veya Ekip Değişikliği",
        BasvuruTuru.OnaylanmisProjeDevami => "Daha Önce Onaylanmış Bir Projenin Devamı",
        _ => t.ToString(),
    };

    public static string Ad(ArastirmaNiteligi n) => n switch
    {
        ArastirmaNiteligi.BilimselArastirmaProjesiBap => "Bilimsel Araştırma Projesi (BAP)",
        ArastirmaNiteligi.YuksekLisansTezi => "Yüksek Lisans Tez Projesi",
        ArastirmaNiteligi.DoktoraSanattaYeterlilikTezi => "Doktora/Sanatta Yeterlilik Tez Projesi",
        ArastirmaNiteligi.TezsizYuksekLisansProjesi => "Tezsiz Yüksek Lisans Tez Projesi",
        ArastirmaNiteligi.Diger => "Diğer",
        _ => n.ToString(),
    };

    public static string Ad(ArastirmaciRolu r) => r switch
    {
        ArastirmaciRolu.SorumluArastirmaci => "Sorumlu Araştırmacı (Proje Yürütücüsü/Danışman)",
        ArastirmaciRolu.IkinciDanismanTezYazari => "2. Danışman / Tez Yazarı",
        ArastirmaciRolu.EkipUyesi => "Araştırmacı (Ekip)",
        _ => r.ToString(),
    };

    public static string Ad(GiderTuru g) => g switch
    {
        GiderTuru.HizmetAlimi => "Hizmet alımı",
        GiderTuru.DemirbasAlimi => "Demirbaş alımı",
        GiderTuru.SarfMalzemeAlimi => "Sarf malzeme alımı",
        GiderTuru.Diger => "Diğer",
        _ => g.ToString(),
    };

    public static string Ad(DestekKurumTuru k) => k switch
    {
        DestekKurumTuru.Universite => "Üniversite",
        DestekKurumTuru.Tubitak => "TÜBİTAK",
        DestekKurumTuru.Dpt => "DPT",
        DestekKurumTuru.KalkinmaAjansi => "Kalkınma Ajansı",
        DestekKurumTuru.Uluslararasi => "Uluslararası",
        DestekKurumTuru.Diger => "Diğer",
        _ => k.ToString(),
    };

    public static string Ad(KararTuru k) => k switch
    {
        KararTuru.Uygun => "Uygun",
        KararTuru.UygunDegil => "Uygun Değil",
        KararTuru.DuzeltmeIstenir => "Düzeltme İstenir",
        _ => k.ToString(),
    };

    /// <summary>Ör. "Oy birliği ile kabul edildi (3 kabul, 0 red)"</summary>
    public static string KararOzeti(Entities.Karar k) =>
        $"{(k.OyBirligiMi ? "Oy birliği" : "Oy çokluğu")} ile " +
        $"{(k.KararTuru == KararTuru.Uygun ? "kabul edildi" : "reddedildi")} " +
        $"({k.KabulOySayisi} kabul, {k.RedOySayisi} red)";

    public static string EvetHayir(bool? deger) => deger switch
    {
        true => "Evet",
        false => "Hayır",
        null => "Cevaplanmadı",
    };
}
