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
/// Yeni tablolar (Basvuru, Ek, BasvuruHareketi ...) buraya DbSet olarak eklenecek.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<Kullanici>(options)
{
    // Örnek: public DbSet<Basvuru> Basvurular => Set<Basvuru>();

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
    }
}
