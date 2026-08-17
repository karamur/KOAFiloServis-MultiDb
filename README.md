# MKFiloServis

**Çok kiracılı (multi-tenant) filo servis yönetimi ERP platformu** — Blazor Server tabanlı web uygulaması, lisans yönetim aracı ve veri senkronizasyon araçlarından oluşan uçtan uca bir çözüm.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)](https://learn.microsoft.com/aspnet/core/blazor/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/License-Proprietary-red)]()

---

## İçindekiler

- [Genel Bakış](#genel-bakış)
- [Özellikler](#özellikler)
- [Mimari](#mimari)
- [Proje Yapısı](#proje-yapısı)
- [Başlarken](#başlarken)
- [Kurulum Paketleri](#kurulum-paketleri)
- [Yapılandırma](#yapılandırma)
- [Katkı](#katkı)

## Genel Bakış

MKFiloServis; araç filosu işleten firmaların **cari, fatura, bütçe, servis operasyonu, araç/şoför yönetimi ve puantaj** süreçlerini tek platformda yönetmesini sağlar. Her firma kendi izole veritabanında çalışır (database-per-tenant).

## Özellikler

| Modül | Kapsam |
|---|---|
| **Cari Yönetimi** | Cari listesi, borç/alacak takibi, risk analizi |
| **Faturalar** | Kesilen/gelen faturalar, proforma, stok türü eşleştirme |
| **Bütçe** | Bütçe planlama ve takip |
| **Servis Operasyonu** | Operasyonel süreç ve tedarikçi yönetimi |
| **Araçlar** | Özmal/kiralık araçlar, evrak yönetimi (ruhsat, sigorta, kasko, muayene), bakım-onarım, plaka takip, canlı araç takip (GPS), lastik takip |
| **Puantaj** | İki katmanlı operasyonel puantaj motoru, otomatik işleme (Quartz), anomali tarama |
| **Personel/Şoför** | Özlük ve evrak yönetimi |
| **Sistem** | Çoklu dil, lisanslama, otomatik güncelleme, sağlık kontrolleri, aktivite loglama |

## Mimari

- **Web**: Blazor Server (.NET 10), Kestrel veya IIS (in-process) barındırma
- **Veritabanı**: PostgreSQL — firma başına ayrı veritabanı (multi-tenant), EF Core `IDbContextFactory` deseni
- **Zamanlanmış Görevler**: Quartz.NET (puantaj otomasyonu, e-Fatura senkronizasyonu, veri toplama, anomali tarama)
- **API**: JWT kimlik doğrulamalı REST uçları
- **Güvenlik**: Şifrelenmiş dosya depolama (DPAPI), makine bazlı lisans anahtarları
- **Önbellek**: Redis (opsiyonel)

## Proje Yapısı

```
MKFiloServis-MultiDb/
├── MKFiloServis.Web/            # Blazor Server web uygulaması (ana ERP)
│   └── Tests/PlaywrightSmoke/   # Uçtan uca smoke testleri
├── MKFiloServis.Shared/         # Ortak entity ve sözleşmeler
├── MKFiloServis.LisansDesktop/  # Lisans yönetim merkezi (WinForms, self-contained)
├── MKFiloServis.DataSync/       # Veri aktarım/senkronizasyon aracı
├── setup/                       # Inno Setup paketleme betikleri (build.ps1, *.iss)
└── scripts/                     # Yardımcı betikler (paketleme, sunucu güncelleme)
```

## Başlarken

### Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- Visual Studio 2026 veya üzeri (önerilen)

### Geliştirme Ortamı

```powershell
git clone https://github.com/karamur/MKFiloServis-MultiDb.git
cd MKFiloServis-MultiDb

# Veritabanı bağlantısını yapılandırın
# MKFiloServis.Web/dbsettings.json dosyasını düzenleyin

dotnet run --project MKFiloServis.Web
```

Uygulama varsayılan olarak `http://localhost:5000` üzerinde açılır.

## Kurulum Paketleri

Tüm dağıtım paketleri `setup/build.ps1` ile üretilir:

```powershell
cd setup
.\build.ps1 -Version 1.0.30
```

| Paket | Açıklama |
|---|---|
| `MKFiloServisKurulum-<v>.exe` | **Tam kurulum** — IIS otomatik etkinleştirilir, .NET Hosting Bundle gömülüdür (internet gerekmez) |
| `MKFiloServisKurulumMusteri-<v>.exe` | Müşteri dağıtım paketi (Kestrel, lisans aracı hariç) |
| `MKFiloServisGuncelle-<v>.exe` | Güncelleme paketi |
| `MKLisansArac-<v>.exe` | Lisans yönetim aracı |

> Müşteriye özel lisanslı paketler, Lisans Yönetim Merkezi'ndeki **"Müşteri Kurulum Paketini Hazırla"** akışıyla tek tıkla üretilir.

## Yapılandırma

| Dosya | Amaç |
|---|---|
| `appsettings.json` | Temel uygulama ayarları |
| `appsettings.Production.json` | Üretim ortamı (JWT secret paketlemede otomatik üretilir) |
| `dbsettings.json` | Veritabanı bağlantı ayarları |

## Katkı

Bu depo özel (proprietary) bir projedir. Değişiklik önerileri için lütfen issue açın veya proje sahibiyle iletişime geçin.

---

© 2026 MK Yazılım — Tüm hakları saklıdır.
