<div align="center">

<img src="https://img.shields.io/badge/-KOA%20Filo%20Servis%20MultiDb-1f6feb?style=for-the-badge&logo=bus&logoColor=white" alt="KOA Filo Servis MultiDb" />

# 🚍 KOA Filo Servis — MultiDb

**Database-Per-Firma Mimarisi ile Kurumsal Filo Yönetim Platformu**

_Her firma için ayrı PostgreSQL veritabanı · Hybrid shared/tenant geçiş · Holding konsolidasyon_

<br />

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![Blazor](https://img.shields.io/badge/Blazor-Interactive%20Server-512BD4?style=flat-square&logo=blazor&logoColor=white)](https://learn.microsoft.com/aspnet/core/blazor/)
[![EF Core 10](https://img.shields.io/badge/EF%20Core-10.0-68217A?style=flat-square&logo=microsoft&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14%2B-336791?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org)
[![Quartz.NET](https://img.shields.io/badge/Quartz.NET-3.x-FB7A24?style=flat-square)](https://www.quartz-scheduler.net)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20%2B%20Playwright-25A162?style=flat-square&logo=testinglibrary&logoColor=white)](#-test-stratejisi)
[![License](https://img.shields.io/badge/License-Proprietary-red?style=flat-square)](#-lisans)
[![Version](https://img.shields.io/badge/Version-2.0--preview-blue?style=flat-square)](#-yol-haritası)
[![DB-Per-Tenant](https://img.shields.io/badge/Architecture-DB--Per--Tenant-green?style=flat-square)](#-database-per-firma-mimarisi)

</div>

---

## 📚 İçindekiler

- [Proje Hakkında](#-proje-hakkında)
- [Database-Per-Firma Mimarisi](#-database-per-firma-mimarisi)
- [Öne Çıkan Yetenekler](#-öne-çıkan-yetenekler)
- [Mimari Genel Bakış](#-mimari-genel-bakış)
- [Hızlı Başlangıç](#-hızlı-başlangıç)
- [Yapılandırma](#-yapılandırma)
- [Veritabanı & Migration Stratejisi](#-veritabanı--migration-stratejisi)
- [Test Stratejisi](#-test-stratejisi)
- [Yol Haritası](#-yol-haritası)
- [Katkıda Bulunma](#-katkıda-bulunma)
- [Lisans](#-lisans)

---

## 📌 Proje Hakkında

**KOA Filo Servis**, personel taşımacılığı yapan firmalar için tasarlanmış kurumsal düzeyde bir **Blazor Interactive Server** uygulamasıdır. Araç, şoför, güzergah ve müşteri verilerinden başlayan operasyonel zinciri **günlük puantaj → hakediş → fatura → muhasebe** akışıyla tek bir platformda yönetir.

Tedarikçi araç/personel, kiralık plaka takibi, evrak süre uyarıları, EBYS arşivi ve AI destekli arama gibi sahanın gerçek ihtiyaçları **yerleşik** olarak gelir.

> 🏢 **Çok-firmalı (multi-tenant) altyapı**, rol bazlı yetkilendirme, EBYS belge arşivi ve AI destekli servisler ile gerçek operasyon yüküne ölçeklenir.

### 🎯 Hedef Kullanıcılar

- Personel taşımacılığı işletmeleri (kurumsal servis filoları)
- Karma filo işleten lojistik firmaları (özmal + kiralık + tedarikçi araç)
- Çok-firmalı holding yapılarındaki taşımacılık birimleri

---

## ✨ Öne Çıkan Yetenekler

<details open>
<summary><b>🚐 Filo &amp; Araç Yönetimi</b></summary>

- Şase numarası bazında **tekil araç kartı** + **plaka geçmişi**.
- Sahiplik tipleri: **Özmal / Kiralık / Tedarikçi** — her biri için ayrı işleyiş.
- Kiralık ve komisyonlu araçlar için **detaylı kira/komisyon hesaplama**.
- Araç evrakları (ruhsat, sigorta, kasko, muayene, yetki belgesi, emisyon, koltuk sigortası…) — **çok versiyonlu dosya arşivi**.
- Evrak bitiş tarihleri **tek noktadan tekilleştirilir**, tüm uyarı/rapor/listeye yansır.

</details>

<details>
<summary><b>👥 Personel &amp; Şoför Operasyonu</b></summary>

- Özlük dosyası, ehliyet, MYK, psikoteknik, sağlık raporu süre takibi.
- Personel-araç atama, izin & devamsızlık yönetimi.
- Tedarikçi personeli için ayrı işleyiş (alt yüklenici takibi).

</details>

<details>
<summary><b>🛣️ Güzergah &amp; Puantaj</b></summary>

- **Kurum** ve **Cari** ayrı kavramlar — Cari'siz Kurum bile güzergah açabilir.
- Hiyerarşik **Güzergah → Araç → Günlük Satır** puantaj ekranı.
- Ay filtreli, **rota bazlı toplu onay** akışı.
- Otomatik puantaj üretimi: varsayılan araç/şoför/tedarikçi şablonlarından doldurur.

</details>

<details>
<summary><b>💰 Hakediş, Fatura &amp; Muhasebe</b></summary>

- Güzergah-araç eşleşmelerinden türeyen **Hakediş** ekranı; sütun bazlı filtreler, gelir/gider özeti, detay & puantaj geçişi.
- Fatura kalemleri, tahsilat, banka/kasa hareketleri, masraflar ve mali analiz.
- Aylık & dönemsel **Excel / PDF rapor** çıktıları.

</details>

<details>
<summary><b>🗂️ Belge Yönetim Sistemi (EBYS)</b></summary>

- Gelen / Giden / Personel Özlük / Araç Evrak başta olmak üzere belge kategorileri.
- AI destekli **belge tipi tanıma** ve **semantik arama**.
- Versiyonlu dosya saklama, şifreli güvenli depolama (`SecureFileService`).

</details>

<details>
<summary><b>🔔 Uyarı &amp; Bildirim Sistemi</b></summary>

- Araç evrakları, şoför belgeleri, tedarikçi sözleşmeleri, kiralık plakalar için **merkezi uyarı paneli**.
- **Quartz.NET** tabanlı periyodik tarama ve bildirim üretimi.

</details>

<details>
<summary><b>🤖 AI / Otomasyon</b></summary>

- Araç piyasa araştırma & değerleme servisleri (Ollama / `Microsoft.Extensions.AI`).
- Belge AI servisi: arşivde içerik bazlı arama ve sınıflandırma.
- Otomatik veri senkronizasyon servisi (`KOAFiloServis.DataSync`).

</details>

<details>
<summary><b>🛡️ Kurumsal Altyapı</b></summary>

- **Firma bazlı multi-tenant** veri ayrımı (global EF query filter + `IAktifFirmaProvider`).
- Detaylı **rol & yetki sistemi** (`Permissions`, `RolePermissions`, menü-bazlı).
- Aktivite logları, oturum izleme, **JWT** ile API erişimi.
- Quartz.NET tabanlı zamanlanmış işler.

</details>

---

## 🏗️ Mimari Genel Bakış

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          KOAFiloServis.Web                              │
│  ┌───────────────────────┐  ┌──────────────────┐  ┌─────────────────┐   │
│  │ Blazor Server (UI)    │  │ REST API         │  │ Quartz Jobs     │   │
│  │  • Components/Pages   │  │  • Controllers   │  │  • Background   │   │
│  │  • Layout / NavMenu   │  │  • JWT Auth      │  │    bildirim/    │   │
│  │  • SignalR runtime    │  │                  │  │    snapshot/    │   │
│  └──────────┬────────────┘  └────────┬─────────┘  └────────┬────────┘   │
│             │                        │                     │            │
│             └──────────┬─────────────┴──────────┬──────────┘            │
│                        ▼                        ▼                       │
│  ┌─────────────────────────────────┐  ┌──────────────────────────────┐  │
│  │ Services Katmanı                │  │ Cross-Cutting                │  │
│  │  • İş kuralları                 │  │  • IAktifFirmaProvider       │  │
│  │  • EF Core sorgu/komut          │  │    (tenant context)          │  │
│  │  • Cache (Memory + Redis)       │  │  • AuditLogService           │  │
│  │  • SecureFileService            │  │  • Permission checks         │  │
│  └────────────────┬────────────────┘  └──────────────┬───────────────┘  │
│                   ▼                                  │                  │
│  ┌─────────────────────────────────────────────────┐ │                  │
│  │ ApplicationDbContext (EF Core 10)               │◄┘                  │
│  │  • Global tenant filter (IFirmaTenant)          │                    │
│  │  • Soft-delete filter (BaseEntity.IsDeleted)    │                    │
│  └────────────────┬────────────────────────────────┘                    │
└───────────────────┼─────────────────────────────────────────────────────┘
                    ▼
        ┌───────────────────────┐
        │ PostgreSQL (varsayılan)│
        │ SQL Server / SQLite /  │
        │ MySQL (opsiyonel)      │
        └───────────────────────┘

   Yardımcı Projeler:
   ─────────────────────
   • KOAFiloServis.Shared        → Domain entities + DTO + interfaces
   • KOAFiloServis.DataSync      → Otomatik dış-sistem senkron servisi
   • KOAFiloServis.LisansDesktop → WinForms lisans yöneticisi
   • KOAFiloServis.Tests         → xUnit + Playwright/Selenium E2E
```

---

## 🔁 Veri Akışı (Yüksek Düzey)

```text
   Kurum / Müşteri ──► Güzergah ──► Araç + Şoför Eşleşmesi
                                          │
                                          ▼
                                  Günlük Puantaj
                                          │
                          ┌───────────────┼─────────────────┐
                          ▼               ▼                 ▼
                     Toplu Onay      Hakediş          Fatura / Muhasebe
                                          │
                                          ▼
                                Belge Uyarıları & Raporlar
```

---

## 🧰 Teknoloji Yığını

| Katman | Teknoloji | Versiyon |
| --- | --- | --- |
| Runtime | **.NET** | 10.0 |
| UI | **Blazor Interactive Server**, Bootstrap 5, Bootstrap Icons | — |
| Backend | ASP.NET Core, Razor Components, REST API Controllers | 10.0 |
| ORM | **Entity Framework Core** (Npgsql · SqlServer · Sqlite · Pomelo MySQL) | 10.0 |
| Cache | StackExchange.Redis (opsiyonel) + InMemory | 2.x |
| Arka Plan | **Quartz.NET Hosting** | 3.x |
| Belge | ClosedXML · EPPlus · QuestPDF | — |
| Mail | MailKit | — |
| AI | `Microsoft.Extensions.AI` · OllamaSharp | preview |
| Auth | ASP.NET Core Identity + özel `AppAuthenticationStateProvider` + JWT | — |
| Test | xUnit · Microsoft.Playwright · Selenium WebDriver | — |
| Lisans | WinForms Desktop (`KOAFiloServis.LisansDesktop`) | — |

---

## 📂 Çözüm Yapısı

```
KOAFiloServis.sln
├── KOAFiloServis.Web/                  # Ana Blazor uygulaması (UI + API + servisler)
│   ├── Components/
│   │   ├── Pages/                      # Modül Razor sayfaları (Filo, Puantaj, Hakediş, EBYS...)
│   │   └── Layout/                     # NavMenu, MainLayout
│   ├── Services/                       # İş kuralları (Filo, Puantaj, Hakediş, AuditLog, ...)
│   │   ├── Interfaces/                 # ITenant-less, IFirmaTenant kontratları
│   │   └── ...
│   ├── Controllers/                    # REST API endpoint'leri (JWT korumalı)
│   ├── Data/
│   │   ├── ApplicationDbContext.cs     # Tüm DbSet'ler + global query filter
│   │   ├── DbInitializer.cs            # SQLite/PostgreSQL şema bootstrap
│   │   └── Migrations/                 # EF Core migration tarihçesi
│   ├── Jobs/                           # Quartz arka plan iş tanımları
│   ├── wwwroot/                        # Statik içerik (css, js, images)
│   └── appsettings.json                # Bağlantı, AI, Mail, Quartz, JWT ayarları
│
├── KOAFiloServis.Shared/               # Domain & paylaşılan kütüphane
│   ├── Entities/                       # Tüm EF entity'leri (~80 sınıf)
│   ├── DTO/                            # API & UI veri transfer nesneleri
│   └── Interfaces/                     # IFirmaTenant, IKopyalanabilirTenant, IAktifFirmaProvider
│
├── KOAFiloServis.DataSync/             # Otomatik veri senkronizasyon servisi
├── KOAFiloServis.LisansDesktop/        # Masaüstü lisans yöneticisi (WinForms)
├── KOAFiloServis.Tests/                # xUnit + Playwright E2E + entegrasyon testleri
│
├── docs/                               # Mimari ve faz dokümanları
│   └── TENANT_MIGRATION_PLAN.md        # Tenant göç bookmark'ı
├── setup/                              # WiX / kurulum scriptleri + release notes
├── scripts/                            # PowerShell yardımcı scriptler (deploy-iis-local vb.)
└── CHANGELOG.md                        # Sürüm kayıtları
```

---

## 🚀 Hızlı Başlangıç

### Önkoşullar

| Bileşen | Versiyon | Zorunlu? |
| --- | --- | --- |
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 | ✅ |
| PostgreSQL | 14+ | ✅ (varsayılan; alternatif: SQL Server / SQLite / MySQL) |
| `dotnet-ef` global tool | 9.x+ | ✅ |
| Redis | 7+ | ⛔ Opsiyonel (cache) |
| Ollama / Azure OpenAI | — | ⛔ Opsiyonel (AI servisleri) |
| Node.js | 18+ | ⛔ Opsiyonel (Playwright tarayıcıları için) |

### Kurulum

```pwsh
# 1) Repoyu klonla
git clone https://github.com/karamur/KOAFiloServis-MultiDb.git
cd KOAFiloServis-MultiDb

# 2) EF Core tool (yoksa)
dotnet tool install --global dotnet-ef

# 3) Bağımlılıkları yükle ve derle
dotnet restore
dotnet build

# 4) Bağlantıyı yapılandır
#    KOAFiloServis.Web/appsettings.Development.json içine ConnectionStrings:DefaultConnection ekleyin

# 5) Veritabanı şemasını oluştur (tüm migration'lar)
dotnet ef database update --project KOAFiloServis.Web --startup-project KOAFiloServis.Web

# 6) Uygulamayı çalıştır
dotnet run --project KOAFiloServis.Web
```

➡️ Uygulama varsayılan olarak **`https://localhost:5001`** adresinde başlar.

### Test Çalıştırma

```pwsh
# Tüm testler
dotnet test KOAFiloServis.Tests/KOAFiloServis.Tests.csproj

# Sadece birim testler (Playwright hariç)
dotnet test --filter "Category!=E2E"
```

---

## ⚙️ Yapılandırma

`KOAFiloServis.Web/appsettings.json` içindeki temel bölümler:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=KOAFiloServisV2;Username=postgres;Password=***"
  },
  "DatabaseProvider": "PostgreSQL",          // PostgreSQL | SqlServer | Sqlite | MySQL
  "Jwt": {
    "Key": "...", "Issuer": "...", "Audience": "...", "ExpireMinutes": 480
  },
  "Quartz": { "Enabled": true },
  "Cache": { "Provider": "Memory" },         // Memory | Redis
  "AI": {
    "Provider": "Ollama",                    // Ollama | AzureOpenAI | Disabled
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.1"
  },
  "Mail": { "Host": "...", "Port": 587, "From": "..." }
}
```

> 🔐 Üretim ortamında **secrets** için `dotnet user-secrets` veya environment variables (`ASPNETCORE_…`) kullanın.

---

## 🏢 Database-Per-Firma Mimarisi

Bu proje (**KOAFiloServis-MultiDb**), orijinal `KOAFiloServis` projesinin **Database-Per-Tenant** mimarisiyle yükseltilmiş versiyonudur.

### Mimari

```
PostgreSQL Server
├── KOAFiloServis_Master     → Firmalar, Kullanicilar, Lisans, Roller (global)
├── Koa_USTUN_GRUP_001       → ÜSTÜN GRUP SEYAHAT (tam izolasyon)
├── Koa_RECEP_USTUN_003      → RECEP ÜSTÜN (tam izolasyon)
└── Koa_USTUN_FILO_005       → ÜSTÜN FİLO TURİZM (tam izolasyon)
```

### Hybrid Geçiş Modeli

- `Firma.DatabaseName == null` → Shared DB modu (eski sistem, `IFirmaTenant` query filter ile)
- `Firma.DatabaseName != null` → Dedicated tenant DB (fiziksel izolasyon)
- **Startup'ta otomatik tenant DB oluşturma** — tüm aktif firmalar için
- **Otomatik veri göçü** — shared DB'den tenant DB'ye lookup + tenant veri kopyalama

### Avantajlar

- 🔒 **Fiziksel veri izolasyonu** — her firma kendi veritabanında
- 📦 **Firma bazlı yedekleme/geri yükleme**
- ⚖️ **KVKK/Compliance** — veriler fiziksel olarak ayrı
- 🚀 **Ölçeklenebilirlik** — firmalar farklı sunuculara dağıtılabilir

### Teknik Detaylar

| Bileşen | Açıklama |
|---------|-----------|
| `MasterDbContext` | Global tablolar (Firmalar, Kullanicilar, Lisans, Roller, RolYetkileri) |
| `ApplicationDbContext` | Tenant verileri (tüm IFirmaTenant entity'leri) |
| `TenantDbContextFactory` | Aktif firmaya göre dinamik connection string |
| `ITenantConnectionStringProvider` | Connection string çözümleyici |
| `TenantDatabaseService` | Tenant DB oluşturma, migration, veri göçü |
| `ITenantDatabaseService` | Admin panelinden tenant DB yönetimi |

### Geliştirme Fazları

| Faz | Durum | Commit |
|-----|:-----:|--------|
| **Faz 1** — Altyapı (entity, factory, DI) | ✅ | `cba5d90` |
| **Faz 2** — Master DB fiziksel ayrım | ✅ | `2de0ef4` |
| **Faz 3** — Tenant DB UI + veri göçü | ✅ | `0261aa6` |
| **Faz 4** — IFirmaTenant temizliği | ⚪ | — |
| **Faz 5** — Holding konsolidasyon | ⚪ | — |

> 📜 **Geçmiş:** Orijinal `KOAFiloServis` projesinde tenant izolasyonu `IFirmaTenant` + EF Core Global Query Filter ile sağlanıyordu. Bu repo, Database-Per-Firma mimarisine geçiş için fork'lanmıştır. Orijinal proje: [`karamur/KOAFiloServis`](https://github.com/karamur/KOAFiloServis).

---

## 🗄️ Veritabanı & Migration Stratejisi

### EF Core Migration Adlandırma

- Tenant göç fazları: `TenantA_*`, `TenantC_*`, `TenantB3_*`, `TenantB4a_*`, `TenantB4b_*`…
- Şema değişiklikleri: `<Faz>_<KısaAçıklama>` (PascalCase).

### Idempotent PL/pgSQL Migration Şablonu

Kritik veri taşıma migration'ları **PL/pgSQL `DO $$ ... $$` blokları** ile idempotent yazılır:

```sql
DO $$
DECLARE r RECORD;
BEGIN
    -- FK'leri dinamik sil
    FOR r IN
        SELECT conname FROM pg_constraint
        WHERE conrelid = '"Araclar"'::regclass
          AND contype = 'f'
          AND conname ILIKE '%SirketId%'
    LOOP
        EXECUTE format('ALTER TABLE "Araclar" DROP CONSTRAINT IF EXISTS %I', r.conname);
    END LOOP;

    -- Kolonu sil (varsa)
    IF EXISTS (SELECT 1 FROM information_schema.columns
               WHERE table_name = 'Araclar' AND column_name = 'SirketId') THEN
        ALTER TABLE "Araclar" DROP COLUMN "SirketId";
    END IF;
END $$;
```

Bu yaklaşım sayesinde:

- ✅ Aynı migration **birden çok kez çalıştırılabilir**.
- ✅ Snapshot-DB drift'lerinde otomatik düzelir.
- ✅ Çoklu ortam (dev/test/prod) tutarlılığı garanti edilir.

### Yedekleme (Üretim)

```pwsh
pg_dump -h <host> -U <user> -d KOAFiloServisV2 -F c `
        -f "backup-$(Get-Date -Format yyyy-MM-dd-HHmm).dump"
```

> ⚠️ **Yıkıcı migration'lardan ÖNCE backup zorunludur** (kolon drop / tablo drop).

---

## 🧪 Test Stratejisi

| Tür | Çatı | Kapsam |
| --- | --- | --- |
| Birim Testler | xUnit | Servis kuralları, hesaplama, mapping |
| Entegrasyon | xUnit + EF InMemory / SQLite | Repository + servis zinciri |
| E2E (UI) | Microsoft.Playwright | Kritik akışlar (login, puantaj, hakediş) |
| Smoke | Selenium WebDriver | Tarayıcı çapraz uyumluluk |

```pwsh
# Sadece birim test
dotnet test --filter "Category=Unit"

# E2E Playwright (önce playwright install)
pwsh KOAFiloServis.Tests/bin/Debug/net10.0/playwright.ps1 install
dotnet test --filter "Category=E2E"
```

---

## 📦 Kurulum / Deploy

### Setup Paketi (Windows)

```pwsh
# setup\build.ps1 — WiX tabanlı kurulum paketi üretir
.\setupolustur.bat
# Çıktı: setup\output\v<X.Y.Z>\KOAFiloServisKurulum-<X.Y.Z>.exe
```

### IIS'e Deploy (Yerel)

```pwsh
# Admin PowerShell:
.\scripts\deploy-iis-local.ps1
```

### Sürüm Notları

- 📝 [`CHANGELOG.md`](CHANGELOG.md) — Tüm sürümler
- 📝 [`setup/RELEASE-NOTES-v1.0.20.md`](setup/RELEASE-NOTES-v1.0.20.md) — En son sürüm

---

## 🔐 Güvenlik

- 🔑 ASP.NET Core Identity tabanlı kullanıcı/rol modeli.
- 🛂 `Permissions` + `RolePermissions` ile **menü & aksiyon bazlı yetkilendirme**.
- 🪪 **JWT** tabanlı API erişimi.
- 📄 `SecureFileService` ile **diskte şifrelenmiş** belge saklama.
- 🧾 Kapsamlı **aktivite logu** (`AuditLogService` — görüntüleme, oluşturma, düzenleme, silme).
- 🔒 İki faktörlü doğrulama (2FA) altyapısı `KullaniciVeLisans` entity'sinde mevcut.
- 🚫 Soft-delete (`BaseEntity.IsDeleted`) ile **kalıcı kayıp önlenir**.

> 🐞 Güvenlik açığı bildirmek için lütfen önce e-posta yoluyla iletişime geçin (public issue **açmayın**).

---

## 🗺️ Yol Haritası

- [ ] 📱 Mobil (MAUI) şoför uygulaması
- [ ] 📊 PowerBI bağlantısı için OData uçları
- [ ] 🌐 Daha kapsamlı tedarikçi self-servis portalı
- [ ] 🧾 SAP / e-Fatura entegrasyonu
- [ ] 🤖 AI tabanlı puantaj anomali tespiti
- [ ] 🌍 Çoklu dil desteği (i18n)

---

## 🤝 Katkıda Bulunma

Bu repo özel bir projeye aittir. Katkı talepleri için lütfen önce bir _issue_ açarak iletişime geçin.

```pwsh
git checkout -b feature/yeni-ozellik
# … geliştirme …
dotnet build && dotnet test
git commit -m "feat(modul): kısa açıklama"
git push origin feature/yeni-ozellik
```

### Commit Mesaj Konvansiyonu

`<tip>(<modul>): <kısa açıklama>` formatı tercih edilir.

| Tip | Anlam |
| --- | --- |
| `feat` | Yeni özellik |
| `fix` | Hata düzeltme |
| `refactor` | Davranış değiştirmeyen iyileştirme |
| `tenant` | Multi-tenant göç adımı |
| `docs` | Dokümantasyon |
| `build` / `chore` | Build/CI/setup |
| `test` | Test ekleme/güncelleme |

---

## 📄 Lisans

© **Karamur Yazılım**. Tüm hakları saklıdır.

Bu yazılım yalnızca lisanslı kullanım için sunulur; izinsiz **kopyalanması, dağıtılması veya türev çalışma üretilmesi yasaktır**.

---

<div align="center">

**KOA Filo Servis**
_Operasyondan muhasebeye, filodan hakedişe — tek panelden uçtan uca yönetim._

<sub>Made with ❤️ on .NET 10 &amp; Blazor</sub>

</div>
