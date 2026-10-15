# Etik Kurul Otomasyonu: Frontend (React)

**Teknolojiler:** React 19 · TypeScript · Vite · React Router

## Bir kere kurulacak
- **Node.js** (LTS sürümü): nodejs.org → "LTS" yazan sürümü indirip kurun.
  Terminalde `node --version` yazınca sürüm görünmeli.

## Çalıştırma
Backend (`dotnet run`) ve Docker'daki veritabanı **açık olmalı**. Sonra **ikinci bir terminal** açın:

```bash
cd frontend
npm install      # sadece ilk seferde ve paketler değişince
npm run dev
```

Tarayıcıda **http://localhost:5173** adresini açın.

> Frontend `/api` ile başlayan istekleri otomatik olarak backend'e (http://localhost:5168) iletir
> (`vite.config.ts` → proxy). Bu yüzden CORS ayarıyla uğraşmanız gerekmez.

## Klasör yapısı

```
src/
├── main.tsx                 → Uygulamanın başlangıcı
├── App.tsx                  → Sayfa adresleri (route'lar)
├── index.css                → Bütün görünüm (renkler, yazı tipleri, düzen)
├── api/
│   ├── tipler.ts            → Backend DTO'larının TypeScript karşılıkları
│   ├── istemci.ts           → fetch + token + hata mesajları
│   ├── servisler.ts         → Her uç noktanın bir fonksiyonu (basvuruApi.kaydet(...) gibi)
│   └── etiketler.ts         → "YuksekLisansTezi" → "Yüksek lisans tez projesi"
├── oturum/OturumBaglami.tsx → Giriş yapan kullanıcı bilgisi
├── bilesenler/              → Üst menü, bildirim kutusu, durum kaşesi, Evet/Hayır...
└── sayfalar/
    ├── GirisSayfasi.tsx, KayitSayfasi.tsx
    ├── BasvurularimSayfasi.tsx
    ├── AdminSayfasi.tsx     → Kullanıcılar ve yetkiler
    └── basvuru/
        ├── BasvuruSayfasi.tsx → 9 adımlı sihirbaz (kaydetme, adımlar arası geçiş)
        ├── Adimlar.tsx        → 1–7. adımların formları
        ├── Onizleme.tsx       → 8. adım: eksik kontrolü, taslak PDF, kilitleme
        └── Belgeler.tsx       → 9. adım: imzalı belge yükleme, gönderme
```

## Yeni bir backend uç noktası eklenince
1. `api/tipler.ts` → yeni DTO'nun tipini ekleyin
2. `api/servisler.ts` → çağıran fonksiyonu ekleyin
3. Sayfada o fonksiyonu kullanın
