using EtikKurul.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtikKurul.Api.Data;

/// <summary>
/// Veritabanı ile konuşan sınıf.
/// IdentityDbContext'ten miras aldığı için kullanıcı ve rol tabloları
/// (AspNetUsers, AspNetRoles, AspNetUserRoles ...) otomatik oluşur.
///
/// SAHİBİ: Veritabanı sorumlusu.
/// Tablo ayrıntıları (uzunluklar, ilişkiler): Data/Yapilandirmalar.cs
/// Sabit veriler (ek türleri, seçenekler):    Data/SabitVeriler.cs
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Kullanici>(options)
{
    // ───── Başvuru ve formun tekrarlanan bölümleri ─────
    public DbSet<Basvuru> Basvurular => Set<Basvuru>();
    public DbSet<BasvuruArastirmacisi> BasvuruArastirmacilari => Set<BasvuruArastirmacisi>();
    public DbSet<VeriToplamaYeri> VeriToplamaYerleri => Set<VeriToplamaYeri>();
    public DbSet<BasvuruDestekKurumu> BasvuruDestekKurumlari => Set<BasvuruDestekKurumu>();
    public DbSet<GiderKalemi> GiderKalemleri => Set<GiderKalemi>();
    public DbSet<IsPaketi> IsPaketleri => Set<IsPaketi>();
    public DbSet<Risk> Riskler => Set<Risk>();
    public DbSet<LiteraturKaynagi> LiteraturKaynaklari => Set<LiteraturKaynagi>();

    // ───── Çoklu seçimler ─────
    public DbSet<Secenek> Secenekler => Set<Secenek>();
    public DbSet<BasvuruSecenek> BasvuruSecenekleri => Set<BasvuruSecenek>();

    // ───── Belgeler ─────
    public DbSet<EkTuru> EkTurleri => Set<EkTuru>();
    public DbSet<Ek> Ekler => Set<Ek>();

    // ───── İş akışı, karar, bildirim ─────
    public DbSet<BasvuruHareketi> BasvuruHareketleri => Set<BasvuruHareketi>();
    public DbSet<Karar> Kararlar => Set<Karar>();
    public DbSet<Bildirim> Bildirimler => Set<Bildirim>();

    protected override void ConfigureConventions(ModelConfigurationBuilder ayarlar)
    {
        // Enum'lar veritabanında metin olarak saklansın ("Dekanlikta" gibi), sayı olarak değil.
        ayarlar.Properties<Enum>().HaveConversion<string>().HaveMaxLength(50);
        // Para alanları: 18 basamak, virgülden sonra 2 hane
        ayarlar.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Identity tabloları için şart, silmeyin.

        builder.Entity<Kullanici>(k =>
        {
            k.Property(x => x.Ad).HasMaxLength(100).IsRequired();
            k.Property(x => x.Soyad).HasMaxLength(100).IsRequired();
            k.Property(x => x.CalistigiBirim).HasMaxLength(200).IsRequired();
            k.Property(x => x.KurumsalEmail).HasMaxLength(256).IsRequired();
            k.Property(x => x.Adres).HasMaxLength(500);
            k.HasIndex(x => x.KurumsalEmail).IsUnique();
        });

        // Data/Yapilandirmalar.cs içindeki bütün tablo ayarlarını uygula
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
