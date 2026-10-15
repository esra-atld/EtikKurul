# Etik Kurul Otomasyonu – Backend (Web API)

Selçuk Üniversitesi Teknoloji Fakültesi Bilimsel Etik Değerlendirme Kurulu başvuru otomasyonu.

**Teknolojiler:** ASP.NET Core Web API (.NET 10) · Entity Framework Core · MSSQL (Docker) · JWT · Swagger

---

## 1. Bir kere kurulacaklar

| Program | Nereden | Not |
|---|---|---|
| **.NET 10 SDK** | dotnet.microsoft.com/download | "SDK" yazanı indirin, "Runtime" değil |
| **VS Code** | code.visualstudio.com | |
| **C# Dev Kit** eklentisi | VS Code → sol menüde Extensions (4 kare ikonu) → "C# Dev Kit" | Microsoft'un olanı |
| **Docker Desktop** | docker.com/products/docker-desktop | Kurulumdan sonra programı açık tutun |
| **Git** | git-scm.com | |

Sonra bir terminal açıp **bir kere** şunu çalıştırın (migration komutları için gerekli):

```bash
dotnet tool install --global dotnet-ef
```

> Terminali kapatıp yeniden açın. `dotnet ef --version` yazınca sürüm görünüyorsa tamam.

---

## 2. Projeyi çalıştırma

VS Code'da **File → Open Folder** ile `EtikKurul` klasörünü açın. Üst menüden **Terminal → New Terminal** seçin. Aşağıdaki komutları sırayla oradan yazın.

**a) Veritabanını başlatın** (Docker Desktop açık olmalı)

```bash
docker compose up -d
```

İlk seferde MSSQL indirilir (birkaç dakika sürer). `docker ps` yazınca `etikkurul-mssql` satırında **(healthy)** görünene kadar bekleyin.

**b) Paketleri indirip derleyin**

```bash
dotnet build
```

Sonunda **"Build succeeded"** yazmalı. Kırmızı hata çıkarsa ekran görüntüsünü alın.

**c) İlk migration (SADECE BİR KERE, sadece bir kişi yapar)**

```bash
cd src/EtikKurul.Api
dotnet ef migrations add IlkKurulum
```

`Migrations` klasörü oluşur. Bunu GitHub'a gönderin. **Bundan sonra migration'ları sadece veritabanı sorumlusu oluşturur.**

**d) Çalıştırın**

```bash
dotnet run
```

(Zaten `src/EtikKurul.Api` klasöründeyseniz. Değilseniz önce `cd src/EtikKurul.Api`.)

Uygulama açılırken:
- bekleyen migration'ları veritabanına uygular (tablolar oluşur),
- 5 rolü oluşturur,
- bir admin kullanıcısı oluşturur.

Tarayıcıda **http://localhost:5168/swagger** adresini açın.

---

## 3. Swagger'da ilk test

1. **POST /api/auth/giris** → *Try it out* → şunu yapıştırın → *Execute*
   ```json
   { "email": "admin@selcuk.edu.tr", "sifre": "Admin12345" }
   ```
2. Cevaptaki `token` değerini kopyalayın (tırnaklar hariç).
3. Sayfanın üstündeki **Authorize 🔒** düğmesine basın, token'ı yapıştırın, *Authorize* → *Close*.
4. **GET /api/auth/ben** → *Execute*. Admin bilgilerinizi ve `"roller": ["Admin"]` görüyorsanız giriş sistemi çalışıyor.
5. **POST /api/auth/kayit** ile yeni bir kullanıcı oluşturun (kurumsal e-posta `selcuk.edu.tr` ile bitmeli).
6. **GET /api/admin/kullanicilar** ile listeyi görün, yeni kullanıcının `id`'sini kopyalayın.
7. **PUT /api/admin/kullanicilar/{id}/roller** ile ona rol verin:
   ```json
   { "roller": ["EtikKurul"] }
   ```

---

## 4. Uç noktalar (endpoint) — şimdilik

| Metot | Adres | Kim | Ne yapar |
|---|---|---|---|
| POST | `/api/auth/kayit` | Herkes | Yeni kullanıcı kaydı (rolü "Basvuran" olur) |
| POST | `/api/auth/giris` | Herkes | Giriş, token döner |
| GET | `/api/auth/ben` | Giriş yapmış herkes | Kendi bilgileri ve rolleri |
| GET | `/api/admin/roller` | Admin | Rol listesi |
| GET | `/api/admin/kullanicilar?arama=` | Admin | Kullanıcı listesi |
| PUT | `/api/admin/kullanicilar/{id}/roller` | Admin | Rol atama |
| PUT | `/api/admin/kullanicilar/{id}/aktiflik` | Admin | Aktif/pasif yapma |
| GET | `/api/tanimlar/secenekler` | Giriş yapmış herkes | Formdaki çoklu seçim seçenekleri |
| GET | `/api/tanimlar/ek-turleri` | Giriş yapmış herkes | Dilekçe, Başvuru Formu, Ek-1…Ek-11 |
| GET | `/api/basvurular` | Giriş yapmış herkes | Kendi başvurularım |
| POST | `/api/basvurular` | Giriş yapmış herkes | Yeni taslak başvuru |
| GET | `/api/basvurular/{id}` | Başvuru sahibi | Başvurunun tamamı (kaldığı yerden devam) |
| PUT | `/api/basvurular/{id}` | Başvuru sahibi | Taslağı kaydet (wizard her adımda formun tamamını gönderir) |
| DELETE | `/api/basvurular/{id}` | Başvuru sahibi | Taslağı sil |
| GET | `/api/basvurular/{id}/gerekli-ekler` | Başvuru sahibi | Bu başvuruda hangi ekler zorunlu, kaçı yüklendi |
| GET | `/api/basvurular/{id}/eksikler` | Başvuru sahibi | Formda boş zorunlu alanlar (adım numarasıyla) |
| GET | `/api/basvurular/{id}/pdf` | Başvuru sahibi | Başvuru paketi PDF'i (kilitli değilse "TASLAK" filigranlı) |
| POST | `/api/basvurular/{id}/kilitle` | Başvuru sahibi | Eksik yoksa formu kilitler → imzalı belge bekleniyor |
| POST | `/api/basvurular/{id}/kilidi-ac` | Başvuru sahibi | Tekrar düzenlemek için kilidi açar (yüklenen belgeler silinir) |
| GET | `/api/basvurular/{id}/ekler` | Başvuru sahibi | Yüklenen belgeler |
| POST | `/api/basvurular/{id}/ekler` | Başvuru sahibi | Islak imzalı PDF yükle (multipart) |
| GET | `/api/basvurular/{id}/ekler/{ekId}/dosya` | Başvuru sahibi | Belgeyi indir |
| DELETE | `/api/basvurular/{id}/ekler/{ekId}` | Başvuru sahibi | Belgeyi sil |
| POST | `/api/basvurular/{id}/gonder` | Başvuru sahibi | Zorunlu belgeler tamamsa gönder → Evrak Kayıt |

**Başvuru akışı (başvuran tarafı):**
`Taslak` → (kilitle) → `ImzaliBelgeBekleniyor` → (belgeleri yükle, gönder) → `EvrakKayitta`

**Wizard adımları:** 1 Genel bilgiler · 2 Araştırmacılar · 3 Veri toplama · 4 Destek ve giderler · 5 Proje içeriği · 6 İş paketleri ve riskler · 7 Etik sorular · 8 Önizleme ve çıktı · 9 Belge yükleme ve gönderme

**Roller:** `Basvuran`, `EvrakKayit`, `Dekanlik`, `EtikKurul`, `Admin`

---

## 5. Klasör yapısı ve kim neye dokunur

```
EtikKurul/
├── docker-compose.yml          → MSSQL veritabanı (Docker)
└── src/EtikKurul.Api/
    ├── Program.cs              → Uygulamanın ayarları (Esra)
    ├── appsettings*.json       → Bağlantı adresi, JWT, admin bilgisi
    ├── Constants/Roller.cs     → Rol adları
    ├── Entities/               → Veritabanı tabloları (VERİTABANI SORUMLUSU)
    ├── Data/                   → AppDbContext + başlangıç verisi (VERİTABANI SORUMLUSU)
    ├── Migrations/             → Sadece VERİTABANI SORUMLUSU oluşturur
    ├── Dtos/                   → API'ye giren/çıkan veri şekilleri (Esra)
    ├── Services/               → İş mantığı: token, PDF, e-posta... (Esra)
    └── Controllers/            → Uç noktalar (Esra)
```

Frontend ayrı bir projede olacak ve bu API'yi Swagger'daki adreslerle çağıracak.

---

## 6. Sık karşılaşılan sorunlar

| Hata | Çözüm |
|---|---|
| `dotnet ef` bulunamadı | 1. bölümdeki `dotnet tool install --global dotnet-ef` komutunu çalıştırın, terminali yeniden açın |
| `PendingModelChangesWarning` / "pending changes" | Tablo değişmiş ama migration oluşturulmamış. Veritabanı sorumlusu `dotnet ef migrations add <Isim>` yapmalı |
| "A network-related error" / bağlanamıyor | Docker Desktop açık mı? `docker ps` ile `etikkurul-mssql` (healthy) mi? |
| Port 1433 kullanımda | Bilgisayarda ayrıca SQL Server kurulu. Ya onu durdurun ya da compose'da `"1434:1433"` yapıp bağlantı adresini `localhost,1434` olarak değiştirin |
| Mac (M1/M2/M3) MSSQL açılmıyor | Docker Desktop → Settings → General → "Use Rosetta for x86_64/amd64 emulation" açık olmalı |
| Veritabanını sıfırlamak istiyorum | `docker compose down -v` (TÜM VERİ SİLİNİR), sonra `docker compose up -d` ve `dotnet run` |

---

## 7. Güvenlik notu

`appsettings.Development.json` içindeki şifre, JWT anahtarı ve admin şifresi **sadece geliştirme içindir**. Canlıya alırken hepsi değiştirilmeli ve GitHub'a konmamalı.

---

## Frontend (React)

Ekranlar `frontend/` klasöründe. Backend çalışırken ikinci bir terminalde:

```bash
cd frontend
npm install
npm run dev
```

→ **http://localhost:5173** (ayrıntılar: `frontend/README.md`)
