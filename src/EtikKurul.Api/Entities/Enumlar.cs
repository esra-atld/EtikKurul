namespace EtikKurul.Api.Entities;

// Veritabanında bu değerler METİN olarak saklanır (ör. "Dekanlikta"),
// böylece SSMS'te tabloya bakınca ne olduğu okunur. Ayar: AppDbContext.ConfigureConventions

/// <summary>Başvurunun iş akışındaki yeri. "Evrak nerede?" sorusunun cevabı.</summary>
public enum BasvuruDurumu
{
    Taslak,                 // Başvuran formu dolduruyor
    ImzaliBelgeBekleniyor,  // PDF çıktısı alındı, form kilitlendi, ıslak imzalı taramalar bekleniyor
    EvrakKayitta,           // Gönderildi, Evrak Kayıt'ın kayıt numarası vermesi bekleniyor
    IadeEdildi,             // Eksik/hatalı belge nedeniyle başvurana geri gönderildi
    Dekanlikta,             // Kayda alındı, Dekanlık'ta
    EtikKurulda,            // Dekanlık kurula iletti, inceleniyor
    KararDekanlikta,        // Kurul karar verdi, karar Dekanlık'a döndü
    Sonuclandi,             // Dekanlık kararı başvurana iletti
    Arsivlendi,             // Arşive kaldırıldı
}

/// <summary>Formdaki "Etik Kurula Başvuru Durumu"</summary>
public enum BasvuruTuru
{
    YeniBasvuru,
    TekrarBasvuruRevizyon,
    ProtokolEkipDegisikligi,
    OnaylanmisProjeDevami,
}

/// <summary>Formdaki "Araştırmanın Niteliği"</summary>
public enum ArastirmaNiteligi
{
    BilimselArastirmaProjesiBap,
    YuksekLisansTezi,
    DoktoraSanattaYeterlilikTezi,
    TezsizYuksekLisansProjesi,
    Diger,
}

public enum ArastirmaciRolu
{
    SorumluArastirmaci,      // Proje yürütücüsü / tez danışmanı
    IkinciDanismanTezYazari, // Taahhütnamedeki "2. danışman / tez yazarı"
    EkipUyesi,               // Diğer araştırmacılar
}

public enum GiderTuru
{
    HizmetAlimi,
    DemirbasAlimi,
    SarfMalzemeAlimi,
    Diger,
}

public enum DestekDurumu
{
    Destekleyen,             // "Araştırmayı Destekleyen Kurum/Kuruluşlar"
    DesteklemesiPlanlanan,   // "Araştırmayı Desteklemesi Planlanan Kurum/Kuruluşlar"
}

public enum DestekKurumTuru
{
    Universite,
    Tubitak,
    Dpt,
    KalkinmaAjansi,
    Uluslararasi,
    Diger,
}

public enum LiteraturTuru
{
    SecilmisLiteratur,  // 3. bölüm sonu
    YontemLiteraturu,   // 4. bölüm sonu
}

/// <summary>Çoklu seçim kutularının grupları (Secenek tablosu)</summary>
public enum SecenekGrubu
{
    VeriToplamaAraci,  // Formdaki "Veri Toplama Aracının Türü"
    KatilimciGrubu,    // Soru 17
    Uygulama,          // Soru 18
}

public enum KararTuru
{
    Uygun,
    UygunDegil,
    DuzeltmeIstenir, // Hocayla teyit edilecek
}

/// <summary>Hangi ek ne zaman zorunlu? Kuralın kendisi kodda (servis) uygulanacak.</summary>
public enum EkZorunlulukKurali
{
    HerZaman,
    Opsiyonel,
    ResitOlmayanKatilimciVarsa,
    DestekleyenKurumVarsa,
    TezCalismasiysa,
    HerArastirmaciIcin,
    ProjeVeyaTezIse,
    VeriToplamaAraciVarsa,
    VeriAraciBaskasinaAitse,
    Kullanilmiyor,
}
