using EtikKurul.Api.Entities;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EtikKurul.Api.Services;

/// <summary>
/// Başvuru paketini tek PDF olarak üretir. Kullanıcı bunu yazdırır, ıslak imzalar ve tarayıp yükler.
///   1) Dilekçe
///   2) Etik Kurul Başvuru Formu
///   3) Ek-1 Taahhütname
///   4) Ek-2 Gönüllü Katılım Formu
/// Her sayfanın üstünde başvuru numarası ve QR kod bulunur (Evrak Kayıt taranmış belgeyi
/// hangi başvuruya ait olduğunu buradan bilir). Form kilitlenmeden alınan çıktıda
/// "TASLAK" filigranı çıkar.
///
/// Kaynak: Selçuk Üniversitesi Teknoloji Fakültesi Bilimsel Etik Değerlendirme Kurulu belgeleri.
/// </summary>
public class BasvuruPdfServisi(IConfiguration ayarlar)
{
    private const string Fakulte = "TEKNOLOJİ FAKÜLTESİ";
    private const float YaziBoyutu = 10;

    /// <param name="b">Arastirmacilar, Secimler(+Secenek), VeriToplamaYerleri, DestekKurumlari,
    /// GiderKalemleri, IsPaketleri, Riskler, LiteraturKaynaklari yüklenmiş olmalı.</param>
    /// <param name="ekAdlari">Dilekçede ve formda listelenecek ekler (gerekli ekler)</param>
    public byte[] Olustur(Basvuru b, IReadOnlyList<string> ekAdlari)
    {
        var taslakMi = !b.FormKilitliMi;
        var siteAdresi = (ayarlar["SiteAdresi"] ?? "http://localhost:5173").TrimEnd('/');
        var qr = QrUret($"{siteAdresi}/basvuru/{b.Id}");
        var tarih = TurkiyeSaati.Simdi.ToString("dd.MM.yyyy");

        return Document.Create(belge =>
        {
            belge.Page(sayfa =>
            {
                sayfa.Size(PageSizes.A4);
                sayfa.Margin(1.8f, Unit.Centimetre);
                sayfa.DefaultTextStyle(x => x.FontSize(YaziBoyutu));

                sayfa.Header().Element(c => SayfaBasligi(c, b, qr));

                if (taslakMi)
                {
                    sayfa.Foreground().AlignCenter().AlignMiddle().Rotate(-35)
                        .Text("TASLAK — İMZALAMAYINIZ").FontSize(40).Bold().FontColor(Colors.Red.Lighten4);
                }

                sayfa.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(6);

                    Dilekce(col, b, ekAdlari, tarih);
                    col.Item().PageBreak();
                    BasvuruFormu(col, b, ekAdlari);
                    col.Item().PageBreak();
                    Taahhutname(col, b, tarih);
                    col.Item().PageBreak();
                    GonulluKatilimFormu(col, b);
                });

                sayfa.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                    t.Span($"Başvuru No: {b.Id}  ·  Sayfa ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    // ═══════════════════════════ SAYFA BAŞLIĞI ═══════════════════════════

    private static void SayfaBasligi(IContainer c, Basvuru b, byte[] qr)
    {
        c.BorderBottom(0.5f).BorderColor(Colors.Grey.Medium).PaddingBottom(6).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("T.C. SELÇUK ÜNİVERSİTESİ").Bold().FontSize(9);
                col.Item().Text($"{Fakulte} BİLİMSEL ETİK DEĞERLENDİRME KURULU").FontSize(8);
                col.Item().PaddingTop(4).Text(t =>
                {
                    t.Span("Başvuru No: ").FontSize(8);
                    t.Span(b.Id.ToString()).Bold().FontSize(8);
                    if (!string.IsNullOrEmpty(b.EvrakKayitNo))
                    {
                        t.Span("   Evrak Kayıt No: ").FontSize(8);
                        t.Span(b.EvrakKayitNo).Bold().FontSize(8);
                    }
                });
            });
            row.ConstantItem(55).Image(qr);
        });
    }

    // ═══════════════════════════ 1) DİLEKÇE ═══════════════════════════

    private static void Dilekce(ColumnDescriptor col, Basvuru b, IReadOnlyList<string> ekAdlari, string tarih)
    {
        var sorumlu = Sorumlu(b);

        col.Item().AlignRight().Text(tarih);
        col.Item().PaddingTop(20).AlignCenter().Text("SELÇUK ÜNİVERSİTESİ").Bold();
        col.Item().AlignCenter().Text(Fakulte).Bold();
        col.Item().AlignCenter().Text("BİLİMSEL ETİK DEĞERLENDİRME KURULU BAŞKANLIĞINA").Bold();

        col.Item().PaddingTop(25).Text(t =>
        {
            t.Span("        “");
            t.Span(b.ArastirmaAdi).Bold();
            t.Span("” başlıklı araştırmamın Bilimsel Etik Değerlendirme Kurulu tarafından incelenmesini talep ediyorum.");
        });
        col.Item().Text("        Gereğinin yapılmasını saygılarımla arz ederim.");

        col.Item().PaddingTop(30).AlignRight().Width(220).Column(imza =>
        {
            imza.Item().AlignCenter().Text("Sorumlu Araştırmacı").Bold();
            imza.Item().AlignCenter().Text(sorumlu is null ? "" : AdUnvan(sorumlu));
            imza.Item().PaddingTop(30).AlignCenter().Text("(İmza)").FontColor(Colors.Grey.Darken1);
        });

        col.Item().PaddingTop(30).Text("EKLER").Bold();
        var no = 1;
        foreach (var ek in ekAdlari.Where(a => a != "Dilekçe"))
            col.Item().Text($"{no++}. {ek}");
    }

    // ═══════════════════════════ 2) BAŞVURU FORMU ═══════════════════════════

    private static void BasvuruFormu(ColumnDescriptor col, Basvuru b, IReadOnlyList<string> ekAdlari)
    {
        col.Item().AlignCenter().Text("ETİK KURUL BAŞVURU FORMU").Bold().FontSize(13);
        col.Item().Text(
            "Mühendislik bilimleriyle ilgili proje/tez/araştırma çalışmaları kapsamında etik kurul onayı gerektiren " +
            "arazi çalışmaları, fiziksel müdahale içermeyen gözlemsel ve betimsel nitelikte araştırmalar için başvuru formudur.")
            .FontSize(8).Italic();

        // Genel bilgiler
        Baslik(col, "Genel Bilgiler");
        AlanTablosu(col,
        [
            ("Araştırmanın Adı", b.ArastirmaAdi),
            ("Etik Kurula Başvuru Durumu", Etiketler.Ad(b.BasvuruTuru)),
            ("Araştırmanın Niteliği", b.ArastirmaNiteligi is { } n
                ? Etiketler.Ad(n) + (n == ArastirmaNiteligi.Diger ? $" ({b.ArastirmaNiteligiDiger})" : "")
                : null),
            ("Sorumlu Araştırmacının Bağlı Olduğu Kurum", b.BagliKurum),
        ]);

        // Araştırmacılar
        Baslik(col, "Araştırmacılar");
        col.Item().Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2.1f); // Görevi
                c.RelativeColumn(2.4f); // Unvan, Ad-Soyad
                c.RelativeColumn(2.1f); // Telefon (11 hane tek satıra sığsın)
                c.RelativeColumn(2.6f); // e-posta
                c.RelativeColumn(2.1f); // Görev yeri
                c.RelativeColumn(1.2f); // İmza
            });
            foreach (var h in new[] { "Görevi", "Unvanı, Ad-Soyadı", "Telefon", "e-posta", "Görev yeri / Adres", "İmza" })
                BaslikHucresi(t.Cell(), h);

            foreach (var a in b.Arastirmacilar.OrderBy(a => a.Sira))
            {
                Hucre(t.Cell(), Etiketler.Ad(a.Rol));
                Hucre(t.Cell(), AdUnvan(a));
                Hucre(t.Cell(), a.Telefon);
                Hucre(t.Cell(), a.Email);
                Hucre(t.Cell(), a.GorevYeriAdres);
                t.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).MinHeight(28);
            }
        });

        // Veri toplama
        Baslik(col, "Veri Toplama");
        AlanTablosu(col,
        [
            ("Veri Toplama Aracının Türü", SecimListesi(b, SecenekGrubu.VeriToplamaAraci, b.VeriToplamaAraciDiger)),
            ("Veri Toplanması Planlanan Dönem",
                $"Başlangıç: {Tarih(b.VeriToplamaBaslangic)}    Bitiş: {Tarih(b.VeriToplamaBitis)}"),
            ("Tahmini Araştırma Süresi (ay)", b.TahminiSureAy?.ToString()),
            ("Veri Toplanması Planlanan Yerler, Kurum ve Kuruluşlar",
                Numarali(b.VeriToplamaYerleri.OrderBy(y => y.Sira).Select(y => y.Ad))),
            ("Veri toplama aracı başkasına mı ait?", Etiketler.EvetHayir(b.VeriAraciBaskasinaAitMi)),
        ]);

        // Destek ve giderler
        Baslik(col, "Destek ve Giderler");
        AlanTablosu(col,
        [
            ("Araştırmayı Destekleyen Kurum/Kuruluşlar", DestekListesi(b, DestekDurumu.Destekleyen)),
            ("Araştırmayı Desteklemesi Planlanan Kurum/Kuruluşlar", DestekListesi(b, DestekDurumu.DesteklemesiPlanlanan)),
        ]);
        if (b.GiderKalemleri.Count > 0)
        {
            col.Item().Table(t =>
            {
                t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(5); c.RelativeColumn(2); });
                BaslikHucresi(t.Cell(), "Gider Türü");
                BaslikHucresi(t.Cell(), "Açıklama");
                BaslikHucresi(t.Cell(), "Tutar (TL)");
                foreach (var g in b.GiderKalemleri.OrderBy(g => g.Sira))
                {
                    Hucre(t.Cell(), Etiketler.Ad(g.Tur));
                    Hucre(t.Cell(), g.Aciklama);
                    t.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).AlignRight()
                        .Text(g.Tutar.ToString("N2")).FontSize(9);
                }
                t.Cell().ColumnSpan(2).Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).AlignRight().Text("TOPLAM").Bold();
                t.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4).AlignRight()
                    .Text(b.GiderKalemleri.Sum(g => g.Tutar).ToString("N2")).Bold();
            });
        }
        else
        {
            col.Item().Text("Araştırma Giderleri: Yoktur").FontSize(9);
        }

        // 1–6 Proje içeriği
        Baslik(col, "1. Türkçe ve Yabancı Dilde Özet");
        MetinBolumu(col, "Türkçe Özet", b.TurkceOzet);
        MetinBolumu(col, "Anahtar Kelimeler", b.AnahtarKelimeler);
        MetinBolumu(col, "Yabancı Dilde Özet", b.YabanciDilOzet);
        MetinBolumu(col, "Keywords", b.Keywords);

        Baslik(col, "2. Amaç");
        MetinBolumu(col, null, b.Amac);

        Baslik(col, "3. Araştırmanın Konu, Kapsam ve Literatür Özeti");
        MetinBolumu(col, null, b.KonuKapsamLiteratur);
        MetinBolumu(col, "Seçilmiş Literatür Listesi", Numarali(Literatur(b, LiteraturTuru.SecilmisLiteratur)));

        Baslik(col, "4. Materyal ve Yöntem");
        MetinBolumu(col, null, b.MateryalYontem);
        MetinBolumu(col, "Yöntemlerde Yararlanılan Literatür Listesi", Numarali(Literatur(b, LiteraturTuru.YontemLiteraturu)));

        Baslik(col, "5. Özgün Değer");
        MetinBolumu(col, null, b.OzgunDeger);

        Baslik(col, "6. Yaygın Etki");
        MetinBolumu(col, null, b.YayginEtki);

        // 7 İş paketleri
        Baslik(col, "7. Yönetim Düzeni (İş Paketleri, Görev Dağılımı ve Süreleri)");
        col.Item().Table(t =>
        {
            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(4); c.RelativeColumn(3); c.RelativeColumn(2); });
            BaslikHucresi(t.Cell(), "İP No");
            BaslikHucresi(t.Cell(), "İş Paketinin Adı ve Hedefleri");
            BaslikHucresi(t.Cell(), "Kim(ler) Tarafından");
            BaslikHucresi(t.Cell(), "Süre");
            foreach (var p in b.IsPaketleri.OrderBy(p => p.No))
            {
                Hucre(t.Cell(), p.No.ToString());
                Hucre(t.Cell(), p.Ad);
                Hucre(t.Cell(), p.Kimler);
                Hucre(t.Cell(), p.Sure);
            }
        });

        // 8 Riskler
        Baslik(col, "8. Risk Yönetimi");
        col.Item().Table(t =>
        {
            t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); c.RelativeColumn(); });
            BaslikHucresi(t.Cell(), "İP No");
            BaslikHucresi(t.Cell(), "En Önemli Risk(ler)");
            BaslikHucresi(t.Cell(), "B Planı");
            foreach (var r in b.Riskler.OrderBy(r => r.IsPaketiNo))
            {
                Hucre(t.Cell(), r.IsPaketiNo.ToString());
                Hucre(t.Cell(), r.RiskAciklamasi);
                Hucre(t.Cell(), r.BPlani);
            }
        });

        // 9–18 Etik sorular
        Baslik(col, "Etik Değerlendirme Soruları");
        EtikSoru(col, "9. Araştırma katılımcılara herhangi bir şekilde yanlı/yanlış bilgi vermeyi ya da çalışmanın amacını tamamen gizli tutmayı gerektiriyor mu?",
            b.YanlisBilgiVeyaGizlilikGerekiyorMu, b.YanlisBilgiVeyaGizlilikAciklama);
        EtikSoru(col, "10. Araştırma katılımcıların fiziksel veya ruhsal sağlıklarını tehdit edici sorular içeriyor mu?",
            b.SaglikTehdidiVarMi, b.SaglikTehdidiAciklama);
        EtikSoru(col, "11. Katılımcıların kişilik hakları ve özel bilgileri korunmakta mıdır?",
            b.KisilikHaklariKorunuyorMu, b.KisilikHaklariAciklama);
        EtikSoru(col, "12. Araştırmaya reşit olmayan çocuklar, kısıtlı ya da engelliler katılmakta mıdır?",
            b.ResitOlmayanKatilimciVarMi, b.ResitOlmayanKatilimciAciklama);
        EtikSoru(col, "13. Katılımcılara araştırmanın niteliği hakkında yeterince açık ve anlaşılabilir bilgi verilmekte midir (gönüllü katılım formu)?",
            b.YeterliBilgiVeriliyorMu, b.YeterliBilgiAciklama);
        MetinBolumu(col, "14. Gönüllü katılımı bozacak ve katılımcıları suistimal edecek tehditlere karşı ne gibi önlemler alınmıştır?",
            b.GonulluKatilimOnlemleri);
        MetinBolumu(col, "15. Gönüllülerin katılma ve çıkma/çıkarılma koşulları açık ve net olarak belli midir?",
            b.KatilmaCikmaKosullari);
        MetinBolumu(col, "16. Beklenen katılımcı sayısı (tahmini)", b.BeklenenKatilimciSayisi?.ToString());
        MetinBolumu(col, "17. Katılımcıları en iyi tanımlayan seçenekler",
            SecimListesi(b, SecenekGrubu.KatilimciGrubu, b.KatilimciGrubuDiger));
        MetinBolumu(col, "18. Çalışma kapsamında yer alacak uygulamalar",
            SecimListesi(b, SecenekGrubu.Uygulama, b.UygulamaDiger));

        Baslik(col, "EKLER");
        var no = 1;
        foreach (var ek in ekAdlari.Where(a => a is not "Dilekçe" and not "Etik Kurul Başvuru Formu"))
            col.Item().Text($"{no++}. {ek}");
    }

    // ═══════════════════════════ 3) EK-1 TAAHHÜTNAME ═══════════════════════════

    private static void Taahhutname(ColumnDescriptor col, Basvuru b, string tarih)
    {
        var sorumlu = Sorumlu(b);
        var tezYazari = b.Arastirmacilar.FirstOrDefault(a => a.Rol == ArastirmaciRolu.IkinciDanismanTezYazari);
        var digerleri = b.Arastirmacilar.Where(a => a.Rol == ArastirmaciRolu.EkipUyesi).OrderBy(a => a.Sira).ToList();

        var tur = b.ArastirmaNiteligi switch
        {
            ArastirmaNiteligi.BilimselArastirmaProjesiBap => "Proje",
            ArastirmaNiteligi.YuksekLisansTezi or ArastirmaNiteligi.DoktoraSanattaYeterlilikTezi
                or ArastirmaNiteligi.TezsizYuksekLisansProjesi => "Tez",
            _ => "Diğer",
        };

        col.Item().AlignCenter().Text("Ek-1 TAAHHÜTNAME").Bold().FontSize(13);
        col.Item().AlignRight().Text($"Tarih: {tarih}");

        AlanTablosu(col,
        [
            ("ÇALIŞMANIN ADI", b.ArastirmaAdi),
            ("ÇALIŞMANIN TÜRÜ", $"Proje: [{(tur == "Proje" ? "X" : " ")}]    Tez: [{(tur == "Tez" ? "X" : " ")}]    Diğer: [{(tur == "Diğer" ? "X" : " ")}]"),
            ("ÇALIŞMADAN SORUMLU YÜRÜTÜCÜ / TEZ DANIŞMANI", sorumlu is null ? null : AdUnvan(sorumlu)),
            ("Adres / Telefon / e-Posta", sorumlu is null ? null : Iletisim(sorumlu)),
            ("ÇALIŞMADAN SORUMLU 2. DANIŞMAN / TEZ YAZARI", tezYazari is null ? null : AdUnvan(tezYazari)),
            ("Adres / Telefon / e-Posta", tezYazari is null ? null : Iletisim(tezYazari)),
            ("DİĞER GÖREVLİLER", digerleri.Count == 0 ? null : string.Join(", ", digerleri.Select(AdUnvan))),
        ]);

        col.Item().PaddingTop(6).Column(m =>
        {
            m.Spacing(3);
            foreach (var madde in new[]
                     {
                         "Selçuk Üniversitesi Etik Kurulu Yönergesini okudum. Yönergede belirtilen hususlara uygun olarak çalışacağımı,",
                         "Onay alınmış projelerde ve lisansüstü tezlerde; insanlarla ilgili yapılacak anket, görüşme, gözlem, alan araştırması, uygulama ve incelemelerde sağlık, güvenlik, insan hakları, mevcut mevzuat hükümleri, hukukun genel ilkeleri ve etik açıdan ihlal etmeyeceğimi,",
                         "Çalışmalarımdaki işlemlerde ve çalışma ekibinde yapılacak değişikliklerde S.Ü. Teknoloji Fakültesi Etik Kurulunun iznini alacağımı,",
                         "Bu çalışma süresince, beklenmeyen gelişmeleri derhal S.Ü. Teknoloji Fakültesi Etik Kuruluna bildireceğimi,",
                         "Başvuru formunda verdiğim tüm bilgilerin eksiksiz ve doğru olduğunu,",
                         "Yukarıda gösterilen adresin yasal tebligat adresim olduğunu, adres değişikliği halinde, yazılı olarak yeni adresimi bildirmediğim takdirde, bu adrese yapılacak tebligatları yasal ve usulüne uygun tebligat olarak kabul edeceğimi,",
                         "Bu çalışmada uygulanacak olan veri toplama aracının seçimi ve/veya kullanımı sırasında fikri mülkiyet haklarından kaynaklanan etik kuralları ihlal etmeyeceğimi,",
                     })
            {
                m.Item().Row(r =>
                {
                    r.ConstantItem(12).Text("•");
                    r.RelativeItem().Text(madde).FontSize(9);
                });
            }
        });
        col.Item().PaddingTop(4).Text("Taahhüt ederim/ederiz.").Bold();

        col.Item().PaddingTop(20).Row(r =>
        {
            r.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text("Proje Yürütücüsü / Tez Danışmanı").Bold();
                c.Item().AlignCenter().Text(sorumlu is null ? "(Adı-soyadı)" : AdUnvan(sorumlu));
                c.Item().PaddingTop(30).AlignCenter().Text("(İmza)").FontColor(Colors.Grey.Darken1);
            });
            r.RelativeItem().Column(c =>
            {
                c.Item().AlignCenter().Text("Tez Yazarı*").Bold();
                c.Item().AlignCenter().Text(tezYazari is null ? "(Adı-soyadı)" : AdUnvan(tezYazari));
                c.Item().PaddingTop(30).AlignCenter().Text("(İmza)").FontColor(Colors.Grey.Darken1);
            });
        });
        col.Item().PaddingTop(10).Text("* Lisansüstü tez çalışması başvurularında akademik danışmanın ve tez yazarının ıslak imzasının bulunması gerekmektedir.")
            .FontSize(8).Italic();
    }

    // ═══════════════════════════ 4) EK-2 GÖNÜLLÜ KATILIM FORMU ═══════════════════════════

    private static void GonulluKatilimFormu(ColumnDescriptor col, Basvuru b)
    {
        var sorumlu = Sorumlu(b);
        var uygulamalar = SecimListesi(b, SecenekGrubu.Uygulama, b.UygulamaDiger) ?? "……………";

        col.Item().AlignCenter().Text("Ek-2 GÖNÜLLÜ KATILIM FORMU").Bold().FontSize(13);
        col.Item().AlignRight().Text("Tarih: …… / …… / 20……");

        col.Item().PaddingTop(6).Text(t =>
        {
            t.Span("Bu çalışma, “");
            t.Span(b.ArastirmaAdi).Bold();
            t.Span("” başlıklı bir araştırma çalışması olup ");
            t.Span(b.Amac ?? "……………");
            t.Span(" amacını taşımaktadır. Çalışma, ");
            t.Span(sorumlu is null ? "……………" : AdUnvan(sorumlu));
            t.Span(" tarafından yürütülmektedir.");
        });

        col.Item().PaddingTop(4).Column(m =>
        {
            m.Spacing(3);
            foreach (var madde in new[]
                     {
                         "Bu çalışmaya katılımınız gönüllülük esasına dayanmaktadır.",
                         $"Çalışmanın amacı doğrultusunda, {uygulamalar} yapılarak sizden veriler toplanacaktır.",
                         "İsminizi yazmak ya da kimliğinizi açığa çıkaracak bir bilgi vermek zorunda değilsiniz / araştırmada katılımcıların isimleri gizli tutulacaktır.",
                         "Araştırma kapsamında toplanan veriler, sadece bilimsel amaçlar doğrultusunda kullanılacak, araştırmanın amacı dışında ya da bir başka araştırmada kullanılmayacak ve gerekmesi halinde, sizin (yazılı) izniniz olmadan başkalarıyla paylaşılmayacaktır.",
                         "İstemeniz halinde sizden toplanan verileri inceleme hakkınız bulunmaktadır.",
                         "Sizden toplanan veriler …………………………… yöntemi ile korunacak ve araştırma bitiminde arşivlenecek veya imha edilecektir.",
                         "Veri toplama sürecinde size rahatsızlık verebilecek herhangi bir soru/talep olmayacaktır. Yine de katılımınız sırasında herhangi bir sebepten rahatsızlık hissederseniz çalışmadan istediğiniz zamanda ayrılabileceksiniz. Çalışmadan ayrılmanız durumunda sizden toplanan veriler çalışmadan çıkarılacak ve imha edilecektir.",
                     })
            {
                m.Item().Row(r =>
                {
                    r.ConstantItem(12).Text("•");
                    r.RelativeItem().Text(madde).FontSize(9);
                });
            }
        });

        col.Item().PaddingTop(8).Text("Sorumlu Araştırmacı (Proje Yürütücüsü / Danışman)").Bold();
        KisiTablosu(col, sorumlu is null ? null : AdUnvan(sorumlu), sorumlu?.Telefon, sorumlu?.Email, sorumlu?.GorevYeriAdres, imzaSutunu: false);

        col.Item().PaddingTop(8).Text(
            "Bu çalışmaya tamamen kendi rızamla, istediğim takdirde çalışmadan ayrılabileceğimi bilerek verdiğim bilgilerin bilimsel amaçlarla kullanılmasını kabul ediyorum.");
        col.Item().Text("(Lütfen bu formu doldurup imzaladıktan sonra veri toplayan kişiye veriniz.)").FontSize(8).Italic();

        col.Item().PaddingTop(8).Text("Katılımcı").Bold();
        KisiTablosu(col, null, null, null, null, imzaSutunu: true);
    }

    // ═══════════════════════════ YARDIMCILAR ═══════════════════════════

    private static void Baslik(ColumnDescriptor col, string metin) =>
        col.Item().PaddingTop(8).Background(Colors.Grey.Lighten3).Padding(4).Text(metin).Bold().FontSize(11);

    private static void AlanTablosu(ColumnDescriptor col, IEnumerable<(string Etiket, string? Deger)> alanlar)
    {
        col.Item().Table(t =>
        {
            t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(5); });
            foreach (var (etiket, deger) in alanlar)
            {
                t.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Grey.Lighten4).Padding(4)
                    .Text(etiket).Bold().FontSize(9);
                Hucre(t.Cell(), deger);
            }
        });
    }

    private static void MetinBolumu(ColumnDescriptor col, string? etiket, string? metin)
    {
        if (etiket is not null)
            col.Item().PaddingTop(4).Text(etiket).Bold().FontSize(9);
        col.Item().Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(5)
            .Text(string.IsNullOrWhiteSpace(metin) ? "-" : metin).FontSize(9);
    }

    private static void EtikSoru(ColumnDescriptor col, string soru, bool? cevap, string? aciklama)
    {
        col.Item().PaddingTop(4).Text(soru).Bold().FontSize(9);
        col.Item().Text($"[{(cevap == true ? "X" : " ")}] Evet     [{(cevap == false ? "X" : " ")}] Hayır").FontSize(9);
        if (!string.IsNullOrWhiteSpace(aciklama))
            col.Item().Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(5).Text($"Açıklama: {aciklama}").FontSize(9);
    }

    private static void KisiTablosu(ColumnDescriptor col, string? ad, string? tel, string? eposta, string? adres, bool imzaSutunu)
    {
        col.Item().Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(3);
                if (imzaSutunu) c.RelativeColumn(2);
            });
            BaslikHucresi(t.Cell(), "Unvanı, Ad-Soyadı");
            BaslikHucresi(t.Cell(), "Telefon");
            BaslikHucresi(t.Cell(), "e-posta");
            BaslikHucresi(t.Cell(), "Görev yeri / Kurum adresi");
            if (imzaSutunu) BaslikHucresi(t.Cell(), "İmza");

            foreach (var deger in new[] { ad, tel, eposta, adres })
                t.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).MinHeight(30).Padding(4).Text(deger ?? "").FontSize(9);
            if (imzaSutunu)
                t.Cell().Border(0.5f).BorderColor(Colors.Grey.Medium).MinHeight(30);
        });
    }

    private static void BaslikHucresi(IContainer hucre, string metin) =>
        hucre.Border(0.5f).BorderColor(Colors.Grey.Medium).Background(Colors.Grey.Lighten3).Padding(4)
            .Text(metin).Bold().FontSize(8);

    private static void Hucre(IContainer hucre, string? metin) =>
        hucre.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4)
            .Text(string.IsNullOrWhiteSpace(metin) ? "-" : metin).FontSize(9);

    private static BasvuruArastirmacisi? Sorumlu(Basvuru b) =>
        b.Arastirmacilar.FirstOrDefault(a => a.Rol == ArastirmaciRolu.SorumluArastirmaci);

    private static string AdUnvan(BasvuruArastirmacisi a) =>
        string.IsNullOrWhiteSpace(a.Unvan) ? a.AdSoyad : $"{a.Unvan} {a.AdSoyad}";

    private static string Iletisim(BasvuruArastirmacisi a) =>
        string.Join(" / ", new[] { a.GorevYeriAdres, a.Telefon, a.Email }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static string? SecimListesi(Basvuru b, SecenekGrubu grup, string? digerAciklama)
    {
        var adlar = b.Secimler
            .Where(s => s.Secenek?.Grup == grup)
            .OrderBy(s => s.Secenek!.Sira)
            .Select(s => s.Secenek!.AciklamaGerekirMi && !string.IsNullOrWhiteSpace(digerAciklama)
                ? $"{s.Secenek.Ad} ({digerAciklama})"
                : s.Secenek!.Ad)
            .ToList();
        return adlar.Count == 0 ? null : string.Join(", ", adlar);
    }

    private static string DestekListesi(Basvuru b, DestekDurumu durum)
    {
        var liste = b.DestekKurumlari.Where(d => d.Durum == durum)
            .Select(d => string.IsNullOrWhiteSpace(d.Aciklama) ? Etiketler.Ad(d.KurumTuru) : $"{Etiketler.Ad(d.KurumTuru)}: {d.Aciklama}")
            .ToList();
        return liste.Count == 0 ? "Yoktur" : string.Join(", ", liste);
    }

    private static IEnumerable<string> Literatur(Basvuru b, LiteraturTuru tur) =>
        b.LiteraturKaynaklari.Where(l => l.Tur == tur).OrderBy(l => l.Sira).Select(l => l.Kaynak);

    private static string? Numarali(IEnumerable<string> satirlar)
    {
        var liste = satirlar.Select((s, i) => $"{i + 1}- {s}").ToList();
        return liste.Count == 0 ? null : string.Join("\n", liste);
    }

    private static string Tarih(DateOnly? t) => t?.ToString("dd.MM.yyyy") ?? "-";

    private static byte[] QrUret(string metin)
    {
        using var uretici = new QRCodeGenerator();
        using var veri = uretici.CreateQrCode(metin, QRCodeGenerator.ECCLevel.M);
        return new PngByteQRCode(veri).GetGraphic(10);
    }
}
