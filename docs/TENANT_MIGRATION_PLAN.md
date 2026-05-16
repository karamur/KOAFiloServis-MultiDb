# Tenant (Firma) Mimarisi - Tam Yeniden Yapılandırma

> **Amaç:** Zirve Müşavirlik mantığı. Kullanıcı login olunca firma seçer; o oturum boyunca tüm CRUD/hesaplama
> sadece o firma verisi üzerinde döner. Firmalar birbirine **sızmaz**. İstenirse şirketler arası kopyalama
> (toplu/tekil) ve şirketler arası kasa/banka transferi yapılabilir.
>
> **Dokunulmayacak modüller:** Bütçe, Muhasebe. Bu modüllerin entity'leri global filter'dan muaftır.

---

## Karar Listesi (Sabit, değişmez referans)

| # | Karar |
|---|------|
| K1 | Tek tenant kavramı: `Firma`. Eski `Sirket` / `SirketId` / `TenantService` deprecated. Veri kaybı olmasın diye hemen drop edilmez, aşamalı emekliliğe alınır. |
| K2 | Aktif firma: Blazor Server **scoped** servis (`IAktifFirmaProvider`) + Session cookie. `FirmaService` içindeki `static _aktifFirma` **bug** → düzeltilecek. |
| K3 | `ApplicationDbContext` global query filter (`HasQueryFilter`) → `FirmaId == aktif` otomatik. Servislerde `.Where(FirmaId == ...)` yazılmaz. |
| K4 | `IFirmaTenant` marker interface. `FirmaId` taşıyan tüm entity'ler implemente eder. |
| K5 | Araç sahiplik 3 tip: `Ozmal`, `Kiralik` (kira firmaya gider), `Tedarikci` (masraf tedarikçide; **lastik + belge takip her zaman firmada**). |
| K6 | Kasa/Banka firma bazlı. Şirketler arası transfer ayrı entity (`FirmalarArasiTransfer`). |
| K7 | Bütçe + Muhasebe dokunulmaz, global filter'dan muaf. |
| K8 | Şirketler arası kopyalama: yeni kayıt üretir, `KaynakFirmaId + KaynakKayitId` audit. Hareketler kopyalanmaz, sadece master kartlar. |
| K9 | Migration: kolon nullable ekle → default firma ile doldur → `IsRequired()`'a al. Veri kaybı yok. |

---

## Aşama Durum Tablosu

| Aşama | Açıklama | Durum | Commit/Migration |
|------|----------|------|------------------|
| A | Plan + IFirmaTenant + IAktifFirmaProvider + FirmaService bug fix | ✅ tamam | (commit edilecek) |
| B | Firma.CariId kaldır, Cari.SirketId deprecate, DbContext global filter | ✅ tamam | (commit edilecek) |
| C | Master entity'lere FirmaId zorunlu (Cari, Kurum, Guzergah, Sofor, Arac, BankaHesap, Stok, MasrafKalemi…) | ⏳ devam | - |
| D | AracSahiplikTipi sadeleştirme + masraf sahibi helper | ⬜ bekliyor | - |
| E | Kasa/Banka firma bazlı + FirmalarArasiTransfer | ⬜ bekliyor | - |
| F | FirmaKopyalamaService + UI (toplu/tekil checkbox) | ⬜ bekliyor | - |
| G | Hakediş Puantaj ekranı (Excel benzeri tablo) | ⬜ bekliyor | - |
| H | Login sonrası firma seçim ekranı + üst bar firma değiştirici | ⬜ bekliyor | - |

---

## Aşama A — Yapılacaklar Detay (TAMAM)

- [x] `docs/TENANT_MIGRATION_PLAN.md` (bu dosya)
- [x] `KOAFiloServis.Shared/Entities/IFirmaTenant.cs` — marker interface
- [x] `KOAFiloServis.Web/Services/IAktifFirmaProvider.cs` + `AktifFirmaProvider` impl (scoped)
- [x] `FirmaService` artık `static _aktifFirma` kullanmıyor, provider'a delege ediyor
- [x] `Program.cs`'te `IAktifFirmaProvider` ve `FirmaService` **Scoped** kaydı (eskiden Singleton'dı)
- [x] `dotnet build` geçiyor

## Aşama B — Yapılacaklar Detay (TAMAM)

- [x] `Firma.CariId` `[Obsolete]` işaretlendi (kolon henüz drop edilmedi; veri güvenliği için Aşama F sonrasına ertelendi)
- [x] `Cari.SirketId` ve `Cari.Sirket` `[Obsolete]` işaretlendi (legacy `Sirket` yapısı ileride emekliye)
- [x] `TenantFilterIgnoreAttribute` eklendi (Bütçe/Muhasebe muafiyeti için)
- [x] `ApplicationDbContext` artık `IAktifFirmaProvider`'ı lazy resolve ediyor (`ResolveAktifFirmaProvider`)
- [x] `IFirmaTenant` entity'lere otomatik named query filter (`"Tenant"`) eklendi (`ApplyFirmaTenantQueryFilter`)
- [x] `SaveChanges` / `SaveChangesAsync` artık yeni eklenen `IFirmaTenant` kayıtlarına aktif `FirmaId`'yi otomatik atıyor (`AssignFirmaTenantId`)
- [x] `dotnet build` geçiyor (0 error, 54 obsolete warning — hepsi planlı temizlik)

## Aşama C — Master Entity FirmaId Listesi

| Entity | Şu an FirmaId? | Yapılacak |
|--------|----------------|-----------|
| Cari | ✅ var (opsiyonel) | zorunlu yap |
| Kurum | ❌ yok | ekle, zorunlu |
| Guzergah | ❌ (KurumId var) | ekle, zorunlu |
| Sofor | ✅ var (opsiyonel) | zorunlu yap |
| Arac | ❌ (SirketId var) | FirmaId ekle, zorunlu |
| BankaHesap | kontrol | zorunlu |
| Stok | kontrol | zorunlu |
| MasrafKalemi | kontrol | zorunlu |
| Fatura | kontrol | zorunlu (Cari üzerinden gelir ama explicit olsun) |
| ServisCalisma | kontrol | zorunlu |
| BankaKasaHareket | kontrol | zorunlu |

---

## Yarıda Kaldıysak Buradan Devam

1. Bu dosyadaki **Aşama Durum Tablosu**'na bak.
2. `⏳ devam` olan aşamanın "Yapılacaklar Detay" listesindeki ilk işaretsiz maddeden başla.
3. Aşama bitince satırını `✅ tamam` yap, commit at, bir sonraki aşamayı `⏳ devam` yap.
4. Veri kaybı olmaması için Aşama B-C'deki migration sırasını **bozma** (nullable → doldur → required).

### Şu Anki Devam Noktası (Aşama C İlk Adım)

**Hedef:** Master kart entity'lerine `IFirmaTenant` implement ettir + `FirmaId` kolonu (nullable → doldur → required) adım adım.

**Sıra (önce düşük riskli olan):**

1. `Kurum` — şu an `FirmaId` yok. `IFirmaTenant` ekle, `FirmaId` nullable kolon migration.
2. `Guzergah` — `IFirmaTenant` + nullable `FirmaId`.
3. `Arac` — `IFirmaTenant` + nullable `FirmaId` (legacy `SirketId` korunur, `[Obsolete]`).
4. `Sofor` — zaten `FirmaId` opsiyonel; sadece `IFirmaTenant` implement et.
5. `Cari` — zaten `FirmaId` opsiyonel; `IFirmaTenant` implement et.
6. Veri doldurma scripti: NULL `FirmaId` olan kayıtlar için varsayılan firma ata.
7. Sonra ayrı migration: `FirmaId` `IsRequired()` yap.
8. `BankaHesap`, `Stok`, `MasrafKalemi`, `Fatura`, `ServisCalisma`, `BankaKasaHareket` için tekrar.

**Önemli kural (K7):** Bütçe ve Muhasebe entity'leri `IFirmaTenant` IMPLEMENT ETMEZ. Bu sayede otomatik filter onları atlar. Yanlışlıkla implement edilirse `[TenantFilterIgnore]` ekle.

**Başlamadan önce yap:** Aşama A + B commit'i at:
```
git add docs/TENANT_MIGRATION_PLAN.md \
        KOAFiloServis.Shared/Entities/IFirmaTenant.cs \
        KOAFiloServis.Shared/Entities/TenantFilterIgnoreAttribute.cs \
        KOAFiloServis.Shared/Entities/Firma.cs \
        KOAFiloServis.Shared/Entities/Cari.cs \
        KOAFiloServis.Web/Services/IAktifFirmaProvider.cs \
        KOAFiloServis.Web/Services/FirmaService.cs \
        KOAFiloServis.Web/Data/ApplicationDbContext.cs \
        KOAFiloServis.Web/Program.cs
git commit -m "tenant: Aşama A+B - scoped provider + global IFirmaTenant query filter"
```
