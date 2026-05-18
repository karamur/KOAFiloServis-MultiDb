# KOAFiloServis v1.0.21

**Yayın Tarihi:** 2026-05-19  
**Tema:** Legacy `SirketId` mimarisinin **fiziksel** emekliliği — Faz 5.3-B4 kapanış sprint'i

---

## 📊 Genel Bakış

Bu sürüm, v1.0.20'de **kod düzeyinde** emekliye alınan legacy `Sirket` tenant mimarisinin **veritabanı düzeyinde** kalıcı olarak kaldırılmasını içerir:

- **13 entity dosyasından `int? SirketId` property silindi**
- **20 tablodan `SirketId` kolonu DROP edildi** (PL/pgSQL idempotent)
- **`_LEGACY_Sirketler` ve `_LEGACY_SirketTransferLoglari` tabloları kalıcı DROP edildi**
- **`AuditLog.SirketId` → `FirmaId`** semantik rename tamamlandı
- **Profesyonel README** GitHub'a yayımlandı (mimari diyagram + tenant modeli + migration stratejisi)
- 2 yeni migration PostgreSQL'e başarıyla uygulandı
- Build: **0 error**

---

## 🆕 Yeni Migration'lar

| Migration | Amaç |
|-----------|------|
| `TenantB4a_DropSirketIdColumnsAndRenameAuditLog` | 20 tablodan `SirketId` kolonu drop + FK/index dinamik drop + `AuditLoglar.SirketId` → `FirmaId` RENAME |
| `TenantB4b_DropLegacyTables` | `_LEGACY_Sirketler` ve `_LEGACY_SirketTransferLoglari` tablolarının kalıcı DROP'u |

Her iki migration da PL/pgSQL idempotent yapıdadır (FK/index tarama, `IF EXISTS` blokları).

---

## 🧹 Kod Temizliği

### Entity'ler (13 dosya)
`Arac`, `AracMaliyetSnapshot`, `BankaHesap`, `BankaKasaHareket`, `CariSeferUcreti`, `Guzergah`, `Hakedis`, `Kapasite`, `KullaniciVeLisans`, `Sofor`, `TasimaTedarikci`, `Lastik.cs` (4 alt sınıf), `ServisKontrat.cs` (4 alt sınıf).

> `AuditLog.SirketId` ise silinmedi; semantik koruma amacıyla `FirmaId`'ye **rename** edildi.

### Servisler
- `AuditLogService` → artık `FirmaId` yazar/filtreler
- `AracMaliyetService` / `IAracMaliyetService` → `sirketId` parametreleri kaldırıldı
- `OperasyonelHakedisService` / `IOperasyonelHakedisService` → `sirketId` parametreleri kaldırıldı
- `LastikService`, `SoforService` → kalıntı `SirketId` atama satırları silindi

### Diğer
- `LastikSezonTakip.razor` → build error fix
- `DbInitializer.cs` → legacy Sirket bootstrap blokları temizlendi

---

## 🔒 Veri Güvenliği

- B4 öncesi **tam `pg_dump` yedeği alındı** (kullanıcı onayı: "backup OK")
- `_LEGACY_*` tablolarının içeriği **kalıcı olarak silindi** — geri dönüş yalnızca backup restore ile mümkün
- B4b migration'ın `Down()` metodu bilinçli olarak boş bırakıldı (FK/seed yeniden inşa edilemez)
- Tüm migration'lar PL/pgSQL idempotent (tekrar çalıştırılabilir)

---

## 📦 Mimari Kazanım

| Metrik | v1.0.20 | v1.0.21 |
|--------|---------|---------|
| `SirketId` taşıyan entity sayısı | 14 | **0** (AuditLog FirmaId'ye rename) |
| `SirketId` kolonu olan tablo | 20+ | **0** |
| `_LEGACY_*` tabloları | 2 (RENAME) | **0** (DROP) |
| Legacy `Sirket` tenant kalıntısı | Var | **Yok** |
| Build error | 0 | **0** |

> Tenant izolasyonu artık tek mekanizma üzerinden işliyor: **`IFirmaTenant` + `ApplicationDbContext` global query filter + `IAktifFirmaProvider`**.

---

## ⚠️ Bilinen Borçlar (1.0.22+ için)

- **Faz 5.2** — `Firma.CariId` drop. İş tarafı onayı önerilir (Unvan fallback regresyon riski).
- **Teknik Borç #1** — True Excel grid (Puantaj UX/performans).

---

## 📥 Kurulum

### Yeni kurulum
```
KOAFiloServisKurulum-1.0.21.exe (admin olarak çalıştırın)
```

### Mevcut kurulumu güncelleme
```
KOAFiloServisGuncelle-1.0.21.exe (admin olarak çalıştırın)
```

> **DİKKAT:** Bu sürüm destruktif migration içerir. Mevcut DB için güncelleme öncesi **mutlaka `pg_dump` yedeği** alın.

**SHA256:** `<sha256-hash-buraya>`

---

## 🔗 Detay

- Teknik döküman: `docs/TENANT_MIGRATION_PLAN.md` — "FAZ 5.3-B4 TAMAMLANDI" bölümü
- Önceki sürüm: [`setup/RELEASE-NOTES-v1.0.20.md`](RELEASE-NOTES-v1.0.20.md)
- README: [`README.md`](../README.md) (profesyonel revizyon, commit `3f87167`)
