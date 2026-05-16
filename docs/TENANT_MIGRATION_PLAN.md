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
| B | Firma.CariId kaldır, Cari.SirketId deprecate, DbContext global filter | ⏳ devam | - |
| C | Master entity'lere FirmaId zorunlu (Cari, Kurum, Guzergah, Sofor, Arac, BankaHesap, Stok, MasrafKalemi…) | ⬜ bekliyor | - |
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

## Aşama B — Yapılacaklar Detay

- [ ] `Firma.CariId` kolonu drop migration (varsa veriyi `Notlar` veya log'a yedekle)
- [ ] `Cari.SirketId` opsiyonel, hâlâ var ama deprecated [Obsolete]
- [ ] `ApplicationDbContext.OnModelCreating`'de `IFirmaTenant` entity'lere `HasQueryFilter(e => e.FirmaId == _aktifProvider.AktifFirmaId)` ekle
- [ ] `SaveChanges` interceptor: yeni eklenen `IFirmaTenant` kayıtlarına aktif FirmaId otomatik ata
- [ ] Bütçe + Muhasebe entity'lerine `[NoTenantFilter]` veya filter dışı bırak

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

### Şu Anki Devam Noktası (Aşama B İlk Adım)

**Sonraki adım:** `Firma.CariId` kolonunun analizi ve güvenli kağıtüzerinde kağıt planı:

1. `Firma.cs` içindeki `CariId` alanını [Obsolete] olarak işaretle, fluent config'de hala kolon olarak kalsın.
2. `ApplicationDbContext` içinde `IAktifFirmaProvider`'ı inject et (DbContext factory üzerinden scoped provider'a erişim için custom factory wrapper gerekecek; pattern: `IDbContextFactory<T>` yerine `IAktifFirmaContextFactory`).
3. `OnModelCreating`'de `IFirmaTenant` implementasyonu olan her entity için:
   `modelBuilder.Entity<T>().HasQueryFilter(e => provider.TumFirmalar || e.FirmaId == provider.AktifFirmaId);`
4. Bütçe + Muhasebe namespace'lerindeki entity'lere **dokunma** (zaten `IFirmaTenant` implemente etmiyorlar).
5. `SaveChangesInterceptor` ekle: `Added` state'inde `IFirmaTenant` ise ve `FirmaId == 0` ise `provider.AktifFirmaId` ata.
6. Migration adı: `EnableTenantQueryFilter` (sadece OnModelCreating değişikliği, kolon ekleme yok).

**Başlamadan önce yap:** `git status` temiz mi kontrol et, Aşama A commit'i at:
```
git add docs/TENANT_MIGRATION_PLAN.md \
        KOAFiloServis.Shared/Entities/IFirmaTenant.cs \
        KOAFiloServis.Web/Services/IAktifFirmaProvider.cs \
        KOAFiloServis.Web/Services/FirmaService.cs \
        KOAFiloServis.Web/Program.cs
git commit -m "tenant: Aşama A - IAktifFirmaProvider (scoped) + IFirmaTenant marker"
```
