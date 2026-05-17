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
| C | Master entity'lere FirmaId zorunlu (Cari, Kurum, Guzergah, Sofor, Arac, BankaHesap, Stok, MasrafKalemi…) | ⏳ devam (C1 tamam) | TenantC1_AddFirmaIdToMasterEntities |
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

### C1 — Marker interface + nullable FirmaId (TAMAM)

| Entity | Durum | Not |
|--------|-------|-----|
| Kurum | ✅ IFirmaTenant + FirmaId (yeni kolon) | Migration `TenantC1_AddFirmaIdToMasterEntities` |
| Guzergah | ✅ IFirmaTenant (FirmaId zaten vardı) | SirketId `[Obsolete]` |
| Arac | ✅ IFirmaTenant + FirmaId (yeni kolon) | SirketId `[Obsolete]` |
| Sofor | ✅ IFirmaTenant (FirmaId zaten vardı) | SirketId `[Obsolete]` |
| Cari | ✅ IFirmaTenant (FirmaId zaten vardı) | SirketId `[Obsolete]` (Aşama B) |

### C2 — Veri doldurma + NOT NULL (bekliyor)

| Entity | Durum | Not |
|--------|-------|-----|
| Kurum | ⬜ | NULL kayıtlara varsayılan firma ata, sonra `IsRequired()` |
| Guzergah | ⬜ | aynı |
| Arac | ⬜ | aynı |
| Sofor | ⬜ | aynı |
| Cari | ⬜ | aynı |

### C3 — Kalan entity'ler (bekliyor)

| Entity | Şu an FirmaId? | Yapılacak |
|--------|----------------|-----------|
| BankaHesap | kontrol | Aşama E içinde zaten yapılacak |
| BankaKasaHareket | kontrol | Aşama E içinde |
| Stok | kontrol | C3 |
| MasrafKalemi | kontrol | C3 |
| Fatura | kontrol | C3 (Cari üzerinden gelir ama explicit olsun) |
| ServisCalisma | kontrol | C3 |

---

## Yarıda Kaldıysak Buradan Devam

1. Bu dosyadaki **Aşama Durum Tablosu**'na bak.
2. `⏳ devam` olan aşamanın "Yapılacaklar Detay" listesindeki ilk işaretsiz maddeden başla.
3. Aşama bitince satırını `✅ tamam` yap, commit at, bir sonraki aşamayı `⏳ devam` yap.
4. Veri kaybı olmaması için Aşama B-C'deki migration sırasını **bozma** (nullable → doldur → required).

### Şu Anki Devam Noktası (Aşama C2 — Veri Doldurma)

**Hedef:** C1 ile eklenen nullable `FirmaId` kolonlarındaki NULL kayıtları varsayılan firma ile doldur, sonra C2 migration ile `IsRequired()` yap.

**Önemli yeni karar (K9 güncelleme):** `IFirmaTenant.FirmaId` `int?` olarak tanımlandı (eski `int` deneyince Sofor/Cari'de 60+ derleme hatası çıktı; nullable yaklaşım K9 stratejisine zaten daha uygun). Filter ve SaveChanges buna göre güncellendi.

**Sıradaki adımlar:**

1. Veri scripti (her tenant entity için): `UPDATE Kurumlar SET FirmaId = (SELECT Id FROM Firmalar WHERE VarsayilanFirma = true) WHERE FirmaId IS NULL;` (Cari, Sofor, Guzergah, Arac, Kurum)
2. `Firma.cs` veya seed kodunda "ANA firma" mevcut değilse seed et.
3. Entity'lerde `int? FirmaId` → `int FirmaId` çevirip migration `TenantC2_RequireFirmaIdOnMasterEntities` oluştur. **Ama önce** üst aklımızda 60+ kullanım yerini (`HasValue`/`Value`/`??`) toplu temizle. Aksi halde derleme patlar.
4. Ya da pragmatik seçim: Şimdilik nullable bırak, query filter zaten korumayı sağlıyor. NOT NULL'a geçişi Aşama F (kopyalama) sonrasına erteleyebiliriz.

**Tercih:** Pragmatik seçim. NOT NULL'a geçiş sonra. **Şimdi doğrudan Aşama D'ye geçiyoruz** (Arac sahiplik 3 tip + masraf sahibi helper). C2/C3 daha sonra toplu temizlik arasında yapılır.

**Başlamadan önce yap:** Aşama C1 commit:
```
git add docs/TENANT_MIGRATION_PLAN.md \
        KOAFiloServis.Shared/Entities/IFirmaTenant.cs \
        KOAFiloServis.Shared/Entities/Kurum.cs \
        KOAFiloServis.Shared/Entities/Guzergah.cs \
        KOAFiloServis.Shared/Entities/Arac.cs \
        KOAFiloServis.Shared/Entities/Sofor.cs \
        KOAFiloServis.Shared/Entities/Cari.cs \
        KOAFiloServis.Web/Data/ApplicationDbContext.cs \
        KOAFiloServis.Web/Migrations/20260517000621_TenantC1_AddFirmaIdToMasterEntities.cs \
        KOAFiloServis.Web/Migrations/20260517000621_TenantC1_AddFirmaIdToMasterEntities.Designer.cs \
        KOAFiloServis.Web/Migrations/ApplicationDbContextModelSnapshot.cs
git commit -m "tenant: Aşama C1 - IFirmaTenant on master entities + nullable FirmaId migration"
```
