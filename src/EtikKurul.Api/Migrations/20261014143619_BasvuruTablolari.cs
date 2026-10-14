using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EtikKurul.Api.Migrations
{
    /// <inheritdoc />
    public partial class BasvuruTablolari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Basvurular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuranKullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SonAdim = table.Column<int>(type: "int", nullable: false),
                    FormKilitliMi = table.Column<bool>(type: "bit", nullable: false),
                    UstBasvuruId = table.Column<int>(type: "int", nullable: true),
                    EvrakKayitNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EvrakKayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeneyVeyaBiyolojikMateryalVarMi = table.Column<bool>(type: "bit", nullable: true),
                    ArastirmaAdi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BasvuruTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ArastirmaNiteligi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ArastirmaNiteligiDiger = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BagliKurum = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VeriToplamaAraciDiger = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    VeriToplamaBaslangic = table.Column<DateOnly>(type: "date", nullable: true),
                    VeriToplamaBitis = table.Column<DateOnly>(type: "date", nullable: true),
                    TahminiSureAy = table.Column<int>(type: "int", nullable: true),
                    VeriAraciBaskasinaAitMi = table.Column<bool>(type: "bit", nullable: true),
                    TurkceOzet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnahtarKelimeler = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    YabanciDilOzet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KonuKapsamLiteratur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MateryalYontem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OzgunDeger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YayginEtki = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YanlisBilgiVeyaGizlilikGerekiyorMu = table.Column<bool>(type: "bit", nullable: true),
                    YanlisBilgiVeyaGizlilikAciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaglikTehdidiVarMi = table.Column<bool>(type: "bit", nullable: true),
                    SaglikTehdidiAciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KisilikHaklariKorunuyorMu = table.Column<bool>(type: "bit", nullable: true),
                    KisilikHaklariAciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResitOlmayanKatilimciVarMi = table.Column<bool>(type: "bit", nullable: true),
                    ResitOlmayanKatilimciAciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YeterliBilgiVeriliyorMu = table.Column<bool>(type: "bit", nullable: true),
                    YeterliBilgiAciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GonulluKatilimOnlemleri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KatilmaCikmaKosullari = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BeklenenKatilimciSayisi = table.Column<int>(type: "int", nullable: true),
                    KatilimciGrubuDiger = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UygulamaDiger = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GonderimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SatirVersiyonu = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Basvurular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Basvurular_AspNetUsers_BasvuranKullaniciId",
                        column: x => x.BasvuranKullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Basvurular_Basvurular_UstBasvuruId",
                        column: x => x.UstBasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EkTurleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ZorunlulukKurali = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CokluDosyaMi = table.Column<bool>(type: "bit", nullable: false),
                    SistemUretirMi = table.Column<bool>(type: "bit", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkTurleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Secenekler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Grup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    AciklamaGerekirMi = table.Column<bool>(type: "bit", nullable: false),
                    CocukRizasiGerektirirMi = table.Column<bool>(type: "bit", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secenekler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BasvuruArastirmacilari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdSoyad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    GorevYeriAdres = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasvuruArastirmacilari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasvuruArastirmacilari_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BasvuruDestekKurumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KurumTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasvuruDestekKurumlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasvuruDestekKurumlari_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BasvuruHareketleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    OncekiDurum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    YeniDurum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IslemYapanKullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasvuruHareketleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasvuruHareketleri_AspNetUsers_IslemYapanKullaniciId",
                        column: x => x.IslemYapanKullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BasvuruHareketleri_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: true),
                    AliciKullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AliciEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Konu = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Icerik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GonderimTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BasariliMi = table.Column<bool>(type: "bit", nullable: false),
                    HataMesaji = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bildirimler_AspNetUsers_AliciKullaniciId",
                        column: x => x.AliciKullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bildirimler_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GiderKalemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    Tur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiderKalemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiderKalemleri_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IsPaketleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    No = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Kimler = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Sure = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsPaketleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IsPaketleri_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kararlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    KararNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KararTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KararTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KararVerenKullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DogrulamaKodu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KararBelgesiDosyaAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kararlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kararlar_AspNetUsers_KararVerenKullaniciId",
                        column: x => x.KararVerenKullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Kararlar_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiteraturKaynaklari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    Tur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Kaynak = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiteraturKaynaklari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiteraturKaynaklari_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Riskler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    IsPaketiNo = table.Column<int>(type: "int", nullable: false),
                    RiskAciklamasi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    BPlani = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Riskler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Riskler_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VeriToplamaYerleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeriToplamaYerleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VeriToplamaYerleri_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BasvuruSecenekleri",
                columns: table => new
                {
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    SecenekId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasvuruSecenekleri", x => new { x.BasvuruId, x.SecenekId });
                    table.ForeignKey(
                        name: "FK_BasvuruSecenekleri_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BasvuruSecenekleri_Secenekler_SecenekId",
                        column: x => x.SecenekId,
                        principalTable: "Secenekler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ekler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    EkTuruId = table.Column<int>(type: "int", nullable: false),
                    BasvuruArastirmacisiId = table.Column<int>(type: "int", nullable: true),
                    OrijinalDosyaAdi = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    KayitliDosyaAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BoyutByte = table.Column<long>(type: "bigint", nullable: false),
                    IcerikTuru = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    YukleyenKullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    YuklemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ekler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ekler_AspNetUsers_YukleyenKullaniciId",
                        column: x => x.YukleyenKullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ekler_BasvuruArastirmacilari_BasvuruArastirmacisiId",
                        column: x => x.BasvuruArastirmacisiId,
                        principalTable: "BasvuruArastirmacilari",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Ekler_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ekler_EkTurleri_EkTuruId",
                        column: x => x.EkTuruId,
                        principalTable: "EkTurleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EkTurleri",
                columns: new[] { "Id", "Aciklama", "Ad", "AktifMi", "CokluDosyaMi", "Kod", "Sira", "SistemUretirMi", "ZorunlulukKurali" },
                values: new object[,]
                {
                    { 1, null, "Dilekçe", true, false, "DILEKCE", 1, true, "HerZaman" },
                    { 2, null, "Etik Kurul Başvuru Formu", true, false, "BASVURU_FORMU", 2, true, "HerZaman" },
                    { 3, "Tez çalışmalarında danışman ve tez yazarının ıslak imzası gerekir.", "Ek-1 Taahhütname", true, false, "EK1", 3, true, "HerZaman" },
                    { 4, null, "Ek-2 Gönüllü Katılım Formu", true, false, "EK2", 4, true, "HerZaman" },
                    { 5, "18 yaş altındaki katılımcılar için.", "Ek-3 Çocuk Rıza Formu", true, false, "EK3", 5, false, "ResitOlmayanKatilimciVarsa" },
                    { 6, "Destekleyen kuruluş ile sorumlu araştırmacı arasında sorumlulukların paylaşıldığına dair protokol, varsa sözleşmeler.", "Ek-4 Destekleyen Kuruluş Protokolü / Taahhütnamesi", true, false, "EK4", 6, false, "DestekleyenKurumVarsa" },
                    { 7, null, "Ek-5 Araştırma Broşürü, Proforma Faturalar vb.", true, true, "EK5", 7, false, "Opsiyonel" },
                    { 8, "Enstitüden tez konusunun onaylandığına dair belge ve öğrencinin durumunu gösterir belge.", "Ek-6 Tez Konusu Onayı ve Öğrenci Belgesi", true, true, "EK6", 8, false, "TezCalismasiysa" },
                    { 9, "Her araştırmacı için ayrı dosya.", "Ek-7 Özgeçmişler (ARBİS formatında)", true, true, "EK7", 9, false, "HerArastirmaciIcin" },
                    { 10, "Elektronik başvuruda gerekmiyor (hocayla teyit edilecek).", "Ek-8 İmzalı Formları İçeren Flash Bellek", false, false, "EK8", 10, false, "Kullanilmiyor" },
                    { 11, null, "Ek-9 Proje Örneği (TÜBİTAK, BAP, Lisansüstü Tez)", true, false, "EK9", 11, false, "ProjeVeyaTezIse" },
                    { 12, null, "Ek-10 Veri Toplama Araçları (anket, ölçek vb.)", true, true, "EK10", 12, false, "VeriToplamaAraciVarsa" },
                    { 13, "Kullanılacak veri toplama araçları başkalarına aitse alınan izin.", "Ek-11 Veri Toplama Aracı Kullanım İzni", true, true, "EK11", 13, false, "VeriAraciBaskasinaAitse" }
                });

            migrationBuilder.InsertData(
                table: "Secenekler",
                columns: new[] { "Id", "AciklamaGerekirMi", "Ad", "AktifMi", "CocukRizasiGerektirirMi", "Grup", "Sira" },
                values: new object[,]
                {
                    { 1, false, "Anket", true, false, "VeriToplamaAraci", 1 },
                    { 2, false, "Görüşme", true, false, "VeriToplamaAraci", 2 },
                    { 3, false, "Veri kaynakları taraması", true, false, "VeriToplamaAraci", 3 },
                    { 4, false, "Ölçek (skala) geliştirme çalışması", true, false, "VeriToplamaAraci", 4 },
                    { 5, false, "Ses ve görüntü kayıtları", true, false, "VeriToplamaAraci", 5 },
                    { 6, false, "Dosya taraması", true, false, "VeriToplamaAraci", 6 },
                    { 7, false, "Sistem-model geliştirme çalışması", true, false, "VeriToplamaAraci", 7 },
                    { 8, true, "Diğer", true, false, "VeriToplamaAraci", 8 },
                    { 101, false, "Okulöncesi Çocuklar", true, true, "KatilimciGrubu", 1 },
                    { 102, false, "İlköğretim Öğrencileri", true, true, "KatilimciGrubu", 2 },
                    { 103, false, "Lise Öğrencileri", true, true, "KatilimciGrubu", 3 },
                    { 104, false, "Üniversite Öğrencileri", true, false, "KatilimciGrubu", 4 },
                    { 105, false, "Çocuk İşçiler", true, true, "KatilimciGrubu", 5 },
                    { 106, false, "Yetişkinler", true, false, "KatilimciGrubu", 6 },
                    { 107, false, "Yaşlılar", true, false, "KatilimciGrubu", 7 },
                    { 108, false, "Kadınlar", true, false, "KatilimciGrubu", 8 },
                    { 109, false, "Erkekler", true, false, "KatilimciGrubu", 9 },
                    { 110, false, "İşsiz Yetişkinler", true, false, "KatilimciGrubu", 10 },
                    { 111, false, "Zihinsel Engelli Bireyler", true, false, "KatilimciGrubu", 11 },
                    { 112, false, "Fiziksel Engelli Bireyler", true, false, "KatilimciGrubu", 12 },
                    { 113, false, "Tutuklular", true, false, "KatilimciGrubu", 13 },
                    { 114, true, "Diğer", true, false, "KatilimciGrubu", 14 },
                    { 201, false, "Anket", true, false, "Uygulama", 1 },
                    { 202, false, "Mülakat", true, false, "Uygulama", 2 },
                    { 203, false, "Gözlem", true, false, "Uygulama", 3 },
                    { 204, false, "Bilgisayar ortamında test/anket", true, false, "Uygulama", 4 },
                    { 205, false, "Görüntü kaydı", true, false, "Uygulama", 5 },
                    { 206, false, "Ses kaydı", true, false, "Uygulama", 6 },
                    { 207, false, "Görüşme", true, false, "Uygulama", 7 },
                    { 208, true, "Diğer", true, false, "Uygulama", 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BasvuruArastirmacilari_BasvuruId",
                table: "BasvuruArastirmacilari",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_BasvuruDestekKurumlari_BasvuruId",
                table: "BasvuruDestekKurumlari",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_BasvuruHareketleri_BasvuruId_Tarih",
                table: "BasvuruHareketleri",
                columns: new[] { "BasvuruId", "Tarih" });

            migrationBuilder.CreateIndex(
                name: "IX_BasvuruHareketleri_IslemYapanKullaniciId",
                table: "BasvuruHareketleri",
                column: "IslemYapanKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_BasvuranKullaniciId",
                table: "Basvurular",
                column: "BasvuranKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_Durum",
                table: "Basvurular",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_EvrakKayitNo",
                table: "Basvurular",
                column: "EvrakKayitNo",
                unique: true,
                filter: "[EvrakKayitNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Basvurular_UstBasvuruId",
                table: "Basvurular",
                column: "UstBasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_BasvuruSecenekleri_SecenekId",
                table: "BasvuruSecenekleri",
                column: "SecenekId");

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_AliciKullaniciId",
                table: "Bildirimler",
                column: "AliciKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Bildirimler_BasvuruId",
                table: "Bildirimler",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Ekler_BasvuruArastirmacisiId",
                table: "Ekler",
                column: "BasvuruArastirmacisiId");

            migrationBuilder.CreateIndex(
                name: "IX_Ekler_BasvuruId",
                table: "Ekler",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Ekler_EkTuruId",
                table: "Ekler",
                column: "EkTuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Ekler_KayitliDosyaAdi",
                table: "Ekler",
                column: "KayitliDosyaAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ekler_YukleyenKullaniciId",
                table: "Ekler",
                column: "YukleyenKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_EkTurleri_Kod",
                table: "EkTurleri",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiderKalemleri_BasvuruId",
                table: "GiderKalemleri",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_IsPaketleri_BasvuruId",
                table: "IsPaketleri",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Kararlar_BasvuruId",
                table: "Kararlar",
                column: "BasvuruId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kararlar_DogrulamaKodu",
                table: "Kararlar",
                column: "DogrulamaKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kararlar_KararNo",
                table: "Kararlar",
                column: "KararNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kararlar_KararVerenKullaniciId",
                table: "Kararlar",
                column: "KararVerenKullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_LiteraturKaynaklari_BasvuruId",
                table: "LiteraturKaynaklari",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Riskler_BasvuruId",
                table: "Riskler",
                column: "BasvuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Secenekler_Grup_Sira",
                table: "Secenekler",
                columns: new[] { "Grup", "Sira" });

            migrationBuilder.CreateIndex(
                name: "IX_VeriToplamaYerleri_BasvuruId",
                table: "VeriToplamaYerleri",
                column: "BasvuruId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BasvuruDestekKurumlari");

            migrationBuilder.DropTable(
                name: "BasvuruHareketleri");

            migrationBuilder.DropTable(
                name: "BasvuruSecenekleri");

            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "Ekler");

            migrationBuilder.DropTable(
                name: "GiderKalemleri");

            migrationBuilder.DropTable(
                name: "IsPaketleri");

            migrationBuilder.DropTable(
                name: "Kararlar");

            migrationBuilder.DropTable(
                name: "LiteraturKaynaklari");

            migrationBuilder.DropTable(
                name: "Riskler");

            migrationBuilder.DropTable(
                name: "VeriToplamaYerleri");

            migrationBuilder.DropTable(
                name: "Secenekler");

            migrationBuilder.DropTable(
                name: "BasvuruArastirmacilari");

            migrationBuilder.DropTable(
                name: "EkTurleri");

            migrationBuilder.DropTable(
                name: "Basvurular");
        }
    }
}
