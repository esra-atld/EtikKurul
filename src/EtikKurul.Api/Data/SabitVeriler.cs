using EtikKurul.Api.Entities;

namespace EtikKurul.Api.Data;

/// <summary>
/// Değişmeyen başlangıç verileri: ek türleri ve çoklu seçim seçenekleri.
/// Bunlar migration'a yazılır; veritabanı kurulurken otomatik eklenir.
/// Yeni seçenek eklerseniz yeni bir Id verin ve yeni migration oluşturun.
/// Mevcut Id'leri DEĞİŞTİRMEYİN (eski başvurular bu Id'lere bağlı).
/// </summary>
public static class SabitVeriler
{
    public static readonly EkTuru[] EkTurleri =
    [
        new() { Id = 1,  Sira = 1,  Kod = "DILEKCE",       Ad = "Dilekçe",                       ZorunlulukKurali = EkZorunlulukKurali.HerZaman, SistemUretirMi = true },
        new() { Id = 2,  Sira = 2,  Kod = "BASVURU_FORMU", Ad = "Etik Kurul Başvuru Formu",      ZorunlulukKurali = EkZorunlulukKurali.HerZaman, SistemUretirMi = true },
        new() { Id = 3,  Sira = 3,  Kod = "EK1",  Ad = "Ek-1 Taahhütname",                       ZorunlulukKurali = EkZorunlulukKurali.HerZaman, SistemUretirMi = true,
                Aciklama = "Tez çalışmalarında danışman ve tez yazarının ıslak imzası gerekir." },
        new() { Id = 4,  Sira = 4,  Kod = "EK2",  Ad = "Ek-2 Gönüllü Katılım Formu",             ZorunlulukKurali = EkZorunlulukKurali.HerZaman, SistemUretirMi = true },
        new() { Id = 5,  Sira = 5,  Kod = "EK3",  Ad = "Ek-3 Çocuk Rıza Formu",                  ZorunlulukKurali = EkZorunlulukKurali.ResitOlmayanKatilimciVarsa,
                Aciklama = "18 yaş altındaki katılımcılar için." },
        new() { Id = 6,  Sira = 6,  Kod = "EK4",  Ad = "Ek-4 Destekleyen Kuruluş Protokolü / Taahhütnamesi", ZorunlulukKurali = EkZorunlulukKurali.DestekleyenKurumVarsa,
                Aciklama = "Destekleyen kuruluş ile sorumlu araştırmacı arasında sorumlulukların paylaşıldığına dair protokol, varsa sözleşmeler." },
        new() { Id = 7,  Sira = 7,  Kod = "EK5",  Ad = "Ek-5 Araştırma Broşürü, Proforma Faturalar vb.", ZorunlulukKurali = EkZorunlulukKurali.Opsiyonel, CokluDosyaMi = true },
        new() { Id = 8,  Sira = 8,  Kod = "EK6",  Ad = "Ek-6 Tez Konusu Onayı ve Öğrenci Belgesi", ZorunlulukKurali = EkZorunlulukKurali.TezCalismasiysa, CokluDosyaMi = true,
                Aciklama = "Enstitüden tez konusunun onaylandığına dair belge ve öğrencinin durumunu gösterir belge." },
        new() { Id = 9,  Sira = 9,  Kod = "EK7",  Ad = "Ek-7 Özgeçmişler (ARBİS formatında)",    ZorunlulukKurali = EkZorunlulukKurali.HerArastirmaciIcin, CokluDosyaMi = true,
                Aciklama = "Her araştırmacı için ayrı dosya." },
        new() { Id = 10, Sira = 10, Kod = "EK8",  Ad = "Ek-8 İmzalı Formları İçeren Flash Bellek", ZorunlulukKurali = EkZorunlulukKurali.Kullanilmiyor, AktifMi = false,
                Aciklama = "Elektronik başvuruda gerekmiyor (hocayla teyit edilecek)." },
        new() { Id = 11, Sira = 11, Kod = "EK9",  Ad = "Ek-9 Proje Örneği (TÜBİTAK, BAP, Lisansüstü Tez)", ZorunlulukKurali = EkZorunlulukKurali.ProjeVeyaTezIse },
        new() { Id = 12, Sira = 12, Kod = "EK10", Ad = "Ek-10 Veri Toplama Araçları (anket, ölçek vb.)", ZorunlulukKurali = EkZorunlulukKurali.VeriToplamaAraciVarsa, CokluDosyaMi = true },
        new() { Id = 13, Sira = 13, Kod = "EK11", Ad = "Ek-11 Veri Toplama Aracı Kullanım İzni", ZorunlulukKurali = EkZorunlulukKurali.VeriAraciBaskasinaAitse, CokluDosyaMi = true,
                Aciklama = "Kullanılacak veri toplama araçları başkalarına aitse alınan izin." },
    ];

    public static readonly Secenek[] Secenekler =
    [
        // ── Veri Toplama Aracının Türü (1–99) ──
        new() { Id = 1, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 1, Ad = "Anket" },
        new() { Id = 2, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 2, Ad = "Görüşme" },
        new() { Id = 3, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 3, Ad = "Veri kaynakları taraması" },
        new() { Id = 4, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 4, Ad = "Ölçek (skala) geliştirme çalışması" },
        new() { Id = 5, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 5, Ad = "Ses ve görüntü kayıtları" },
        new() { Id = 6, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 6, Ad = "Dosya taraması" },
        new() { Id = 7, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 7, Ad = "Sistem-model geliştirme çalışması" },
        new() { Id = 8, Grup = SecenekGrubu.VeriToplamaAraci, Sira = 8, Ad = "Diğer", AciklamaGerekirMi = true },

        // ── 17. Katılımcı grupları (101–199) ──
        new() { Id = 101, Grup = SecenekGrubu.KatilimciGrubu, Sira = 1,  Ad = "Okulöncesi Çocuklar",      CocukRizasiGerektirirMi = true },
        new() { Id = 102, Grup = SecenekGrubu.KatilimciGrubu, Sira = 2,  Ad = "İlköğretim Öğrencileri",   CocukRizasiGerektirirMi = true },
        new() { Id = 103, Grup = SecenekGrubu.KatilimciGrubu, Sira = 3,  Ad = "Lise Öğrencileri",         CocukRizasiGerektirirMi = true },
        new() { Id = 104, Grup = SecenekGrubu.KatilimciGrubu, Sira = 4,  Ad = "Üniversite Öğrencileri" },
        new() { Id = 105, Grup = SecenekGrubu.KatilimciGrubu, Sira = 5,  Ad = "Çocuk İşçiler",            CocukRizasiGerektirirMi = true },
        new() { Id = 106, Grup = SecenekGrubu.KatilimciGrubu, Sira = 6,  Ad = "Yetişkinler" },
        new() { Id = 107, Grup = SecenekGrubu.KatilimciGrubu, Sira = 7,  Ad = "Yaşlılar" },
        new() { Id = 108, Grup = SecenekGrubu.KatilimciGrubu, Sira = 8,  Ad = "Kadınlar" },
        new() { Id = 109, Grup = SecenekGrubu.KatilimciGrubu, Sira = 9,  Ad = "Erkekler" },
        new() { Id = 110, Grup = SecenekGrubu.KatilimciGrubu, Sira = 10, Ad = "İşsiz Yetişkinler" },
        new() { Id = 111, Grup = SecenekGrubu.KatilimciGrubu, Sira = 11, Ad = "Zihinsel Engelli Bireyler" },
        new() { Id = 112, Grup = SecenekGrubu.KatilimciGrubu, Sira = 12, Ad = "Fiziksel Engelli Bireyler" },
        new() { Id = 113, Grup = SecenekGrubu.KatilimciGrubu, Sira = 13, Ad = "Tutuklular" },
        new() { Id = 114, Grup = SecenekGrubu.KatilimciGrubu, Sira = 14, Ad = "Diğer", AciklamaGerekirMi = true },

        // ── 18. Uygulamalar (201–299) ──
        new() { Id = 201, Grup = SecenekGrubu.Uygulama, Sira = 1, Ad = "Anket" },
        new() { Id = 202, Grup = SecenekGrubu.Uygulama, Sira = 2, Ad = "Mülakat" },
        new() { Id = 203, Grup = SecenekGrubu.Uygulama, Sira = 3, Ad = "Gözlem" },
        new() { Id = 204, Grup = SecenekGrubu.Uygulama, Sira = 4, Ad = "Bilgisayar ortamında test/anket" },
        new() { Id = 205, Grup = SecenekGrubu.Uygulama, Sira = 5, Ad = "Görüntü kaydı" },
        new() { Id = 206, Grup = SecenekGrubu.Uygulama, Sira = 6, Ad = "Ses kaydı" },
        new() { Id = 207, Grup = SecenekGrubu.Uygulama, Sira = 7, Ad = "Görüşme" },
        new() { Id = 208, Grup = SecenekGrubu.Uygulama, Sira = 8, Ad = "Diğer", AciklamaGerekirMi = true },
    ];
}
