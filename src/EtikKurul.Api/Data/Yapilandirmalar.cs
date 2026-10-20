using EtikKurul.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtikKurul.Api.Data;

// Her tablonun alan uzunlukları, zorunlu alanları, indeksleri ve ilişkileri.
// AppDbContext bu dosyadaki sınıfların hepsini otomatik bulup uygular.
//
// SİLME KURALLARI:
//  - Bir başvuru silinirse (sadece taslaklar silinebilecek) alt tabloları da silinir (Cascade).
//  - Kullanıcı, ek türü, seçenek gibi "başkalarının da kullandığı" kayıtlar
//    bağlı veri varken silinemez (Restrict). Kullanıcılar silinmez, pasif yapılır.

public class BasvuruYapilandirmasi : IEntityTypeConfiguration<Basvuru>
{
    public void Configure(EntityTypeBuilder<Basvuru> b)
    {
        b.Property(x => x.ArastirmaAdi).HasMaxLength(500).IsRequired();
        b.Property(x => x.ArastirmaNiteligiDiger).HasMaxLength(300);
        b.Property(x => x.BagliKurum).HasMaxLength(500);
        b.Property(x => x.EvrakKayitNo).HasMaxLength(50);
        b.Property(x => x.AnahtarKelimeler).HasMaxLength(500);
        b.Property(x => x.Keywords).HasMaxLength(500);
        b.Property(x => x.VeriToplamaAraciDiger).HasMaxLength(300);
        b.Property(x => x.KatilimciGrubuDiger).HasMaxLength(300);
        b.Property(x => x.UygulamaDiger).HasMaxLength(300);
        // Özet, amaç, yöntem gibi uzun metinler sınırsız (nvarchar(max)) kalır.

        b.Property(x => x.SatirVersiyonu).IsRowVersion();

        b.HasIndex(x => x.EvrakKayitNo).IsUnique();
        b.HasIndex(x => x.Durum);
        b.HasIndex(x => x.BasvuranKullaniciId);

        b.HasOne(x => x.BasvuranKullanici)
            .WithMany()
            .HasForeignKey(x => x.BasvuranKullaniciId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.UstBasvuru)
            .WithMany()
            .HasForeignKey(x => x.UstBasvuruId)
            .OnDelete(DeleteBehavior.NoAction);

        b.HasOne(x => x.Karar)
            .WithOne(x => x.Basvuru)
            .HasForeignKey<Karar>(x => x.BasvuruId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BasvuruArastirmacisiYapilandirmasi : IEntityTypeConfiguration<BasvuruArastirmacisi>
{
    public void Configure(EntityTypeBuilder<BasvuruArastirmacisi> b)
    {
        b.Property(x => x.Unvan).HasMaxLength(100);
        b.Property(x => x.AdSoyad).HasMaxLength(200).IsRequired();
        b.Property(x => x.Telefon).HasMaxLength(30);
        b.Property(x => x.Email).HasMaxLength(256);
        b.Property(x => x.GorevYeriAdres).HasMaxLength(500);

        b.HasOne(x => x.Basvuru).WithMany(x => x.Arastirmacilar)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class VeriToplamaYeriYapilandirmasi : IEntityTypeConfiguration<VeriToplamaYeri>
{
    public void Configure(EntityTypeBuilder<VeriToplamaYeri> b)
    {
        b.Property(x => x.Ad).HasMaxLength(300).IsRequired();
        b.HasOne(x => x.Basvuru).WithMany(x => x.VeriToplamaYerleri)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BasvuruDestekKurumuYapilandirmasi : IEntityTypeConfiguration<BasvuruDestekKurumu>
{
    public void Configure(EntityTypeBuilder<BasvuruDestekKurumu> b)
    {
        b.Property(x => x.Aciklama).HasMaxLength(300);
        b.HasOne(x => x.Basvuru).WithMany(x => x.DestekKurumlari)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class GiderKalemiYapilandirmasi : IEntityTypeConfiguration<GiderKalemi>
{
    public void Configure(EntityTypeBuilder<GiderKalemi> b)
    {
        b.Property(x => x.Aciklama).HasMaxLength(300).IsRequired();
        b.HasOne(x => x.Basvuru).WithMany(x => x.GiderKalemleri)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class IsPaketiYapilandirmasi : IEntityTypeConfiguration<IsPaketi>
{
    public void Configure(EntityTypeBuilder<IsPaketi> b)
    {
        b.Property(x => x.Ad).HasMaxLength(300).IsRequired();
        b.Property(x => x.Kimler).HasMaxLength(300);
        b.Property(x => x.Sure).HasMaxLength(100);
        b.HasOne(x => x.Basvuru).WithMany(x => x.IsPaketleri)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class RiskYapilandirmasi : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> b)
    {
        b.Property(x => x.RiskAciklamasi).HasMaxLength(1000).IsRequired();
        b.Property(x => x.BPlani).HasMaxLength(1000);
        b.HasOne(x => x.Basvuru).WithMany(x => x.Riskler)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class LiteraturKaynagiYapilandirmasi : IEntityTypeConfiguration<LiteraturKaynagi>
{
    public void Configure(EntityTypeBuilder<LiteraturKaynagi> b)
    {
        b.Property(x => x.Kaynak).HasMaxLength(1000).IsRequired();
        b.HasOne(x => x.Basvuru).WithMany(x => x.LiteraturKaynaklari)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SecenekYapilandirmasi : IEntityTypeConfiguration<Secenek>
{
    public void Configure(EntityTypeBuilder<Secenek> b)
    {
        b.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        b.HasIndex(x => new { x.Grup, x.Sira });
        b.HasData(SabitVeriler.Secenekler);
    }
}

public class BasvuruSecenekYapilandirmasi : IEntityTypeConfiguration<BasvuruSecenek>
{
    public void Configure(EntityTypeBuilder<BasvuruSecenek> b)
    {
        b.HasKey(x => new { x.BasvuruId, x.SecenekId }); // Aynı seçenek bir başvuruda iki kez işaretlenemez

        b.HasOne(x => x.Basvuru).WithMany(x => x.Secimler)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Secenek).WithMany()
            .HasForeignKey(x => x.SecenekId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EkTuruYapilandirmasi : IEntityTypeConfiguration<EkTuru>
{
    public void Configure(EntityTypeBuilder<EkTuru> b)
    {
        b.Property(x => x.Kod).HasMaxLength(30).IsRequired();
        b.Property(x => x.Ad).HasMaxLength(200).IsRequired();
        b.Property(x => x.Aciklama).HasMaxLength(500);
        b.HasIndex(x => x.Kod).IsUnique();
        b.HasData(SabitVeriler.EkTurleri);
    }
}

public class EkYapilandirmasi : IEntityTypeConfiguration<Ek>
{
    public void Configure(EntityTypeBuilder<Ek> b)
    {
        b.Property(x => x.OrijinalDosyaAdi).HasMaxLength(260).IsRequired();
        b.Property(x => x.KayitliDosyaAdi).HasMaxLength(100).IsRequired();
        b.Property(x => x.IcerikTuru).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.KayitliDosyaAdi).IsUnique();

        b.HasOne(x => x.Basvuru).WithMany(x => x.Ekler)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.EkTuru).WithMany()
            .HasForeignKey(x => x.EkTuruId).OnDelete(DeleteBehavior.Restrict);
        // SQL Server "birden çok silme yolu" hatası vermesin diye NoAction:
        b.HasOne(x => x.BasvuruArastirmacisi).WithMany()
            .HasForeignKey(x => x.BasvuruArastirmacisiId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.YukleyenKullanici).WithMany()
            .HasForeignKey(x => x.YukleyenKullaniciId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BasvuruHareketiYapilandirmasi : IEntityTypeConfiguration<BasvuruHareketi>
{
    public void Configure(EntityTypeBuilder<BasvuruHareketi> b)
    {
        b.Property(x => x.Aciklama).HasMaxLength(1000);
        b.HasIndex(x => new { x.BasvuruId, x.Tarih });

        b.HasOne(x => x.Basvuru).WithMany(x => x.Hareketler)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.IslemYapanKullanici).WithMany()
            .HasForeignKey(x => x.IslemYapanKullaniciId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class KararYapilandirmasi : IEntityTypeConfiguration<Karar>
{
    public void Configure(EntityTypeBuilder<Karar> b)
    {
        b.Property(x => x.KararNo).HasMaxLength(50);
        b.Property(x => x.KararBelgesiDosyaAdi).HasMaxLength(100);
        b.HasIndex(x => x.KararNo).IsUnique();
        b.HasIndex(x => x.DogrulamaKodu).IsUnique();

        b.HasOne(x => x.KararVerenKullanici).WithMany()
            .HasForeignKey(x => x.KararVerenKullaniciId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EtikKurulOyuYapilandirmasi : IEntityTypeConfiguration<EtikKurulOyu>
{
    public void Configure(EntityTypeBuilder<EtikKurulOyu> b)
    {
        // Bir üye bir başvuruya bir kez oy verir
        b.HasIndex(x => new { x.BasvuruId, x.UyeKullaniciId }).IsUnique();
        b.HasIndex(x => x.UyeKullaniciId);

        b.HasOne(x => x.Basvuru).WithMany(x => x.Oylar)
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.UyeKullanici).WithMany()
            .HasForeignKey(x => x.UyeKullaniciId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BildirimYapilandirmasi : IEntityTypeConfiguration<Bildirim>
{
    public void Configure(EntityTypeBuilder<Bildirim> b)
    {
        b.Property(x => x.AliciEmail).HasMaxLength(256).IsRequired();
        b.Property(x => x.Konu).HasMaxLength(300).IsRequired();
        b.Property(x => x.HataMesaji).HasMaxLength(2000);

        b.HasOne(x => x.Basvuru).WithMany()
            .HasForeignKey(x => x.BasvuruId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.AliciKullanici).WithMany()
            .HasForeignKey(x => x.AliciKullaniciId).OnDelete(DeleteBehavior.SetNull);
    }
}
