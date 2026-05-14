# Çalışma Notları — 2026-05-13

> Branch: `main` · Repo: `karamur/KOAFiloServis`
> Bu doküman bugün yapılanları ve yarınki devam adımlarını özetler.

---

## 1) Bugün Tamamlananlar

### A. Puantaj ↔ Fatura Mutabakat İyileştirmeleri
**Dosya:** `KOAFiloServis.Web/Components/Pages/Filo/PuantajMutabakat.razor`
**Commit:** `194c99c` — *PuantajMutabakat: sadece farkli olanlar / tolerans filtresi*

- **"Sadece farklı olanlar" toggle** + **± tolerans alanı** (default `0,01 ₺`).
- Sekme rozetleri `gösterilen / toplam` formatında (`5 / 42`).
- Toplam satırı (`tfoot`) filtreye göre dinamik hesaplanıyor; başlık `(farklı)` / `(tümü)` olarak işaretleniyor.
- CSV export aktif filtreyi takip ediyor (sadece görünen satırlar).
- Boş durum mesajı ayrıştırıldı: gerçekten veri yoksa "Veri yok", filtre yüzünden boşsa tolerans bilgisini içeren mesaj.

### B. Tedarikçi Listesi UX
**Dosya:** `KOAFiloServis.Web/Components/Pages/PersonelTasima/TasimaTedarikciList.razor`
**Commit:** `1ae42ee` — *TasimaTedarikciList: Cari filtresi + toast geri bildirim*

- **Cari durum filtresi**: Hepsi / Yalnızca Eksik / Yalnızca Atanmış.
- Sayaç netleşti: `Gösterilen: X / Toplam Y`.
- **Toast geri bildirim** (tek/toplu atama).
- Toplu atamada **hata toleransı**: tek satırın hatası diğerlerini durdurmuyor; başarı/başarısız ayrı toast.

### C. Firma Yönetimi Aynı Desen
**Dosya:** `KOAFiloServis.Web/Components/Pages/Ayarlar/FirmaYonetimi.razor`
**Commit:** `b11b8aa` — *FirmaYonetimi: arama/durum/Cari filtre toolbar + toplu atama hata toleransi*

- Üst filtre toolbar'ı (arama + durum + cari durumu).
- `FiltreliFirmalar` üzerinden render.
- Toplu atama hata toleransı + ayrı başarı/hata toast'ları.

### D. KRİTİK BUG FIX — `Cari.FirmaId1` Shadow FK
**Dosyalar:**
- `KOAFiloServis.Web/Data/ApplicationDbContext.cs`
- `KOAFiloServis.Web/Data/Migrations/20260513140012_FixCariFirmaShadowFK.cs`

**Commit:** `95c6168` — *Fix: Cari->Firma shadow FK (FirmaId1) sutunu - explicit relationship + temizleme migration*

#### Problem
Dashboard / servis / finans / belge uyarıları sayfalarında:
```
42703: column c.FirmaId1 does not exist
```
Tüm `Cari` SELECT'leri patlıyordu.

#### Kök Neden
`Firma.CariId` için DbContext'te şöyle yazılmıştı:
```csharp
entity.HasOne<Cari>().WithMany().HasForeignKey(e => e.CariId);
```
EF Core convention'ı, `Cari.Firma` navigation'ını da bu yeni ilişkiye bağladı; `Cari.FirmaId` zaten kullanıldığı için `Cari` tarafında shadow FK **`FirmaId1`** üretti. Snapshot'a girdi, ama gerçek DB'de bu sütun yoktu → 42703.

#### Çözüm
1. `Cari` konfigürasyonuna **explicit** ilişki:
```csharp
entity.HasOne(e => e.Firma)
       .WithMany()
       .HasForeignKey(e => e.FirmaId)
       .OnDelete(DeleteBehavior.SetNull);
```
2. Defansif PostgreSQL migration (IF EXISTS / IF NOT EXISTS blokları): `FirmaId1`, ilgili FK ve index güvenle düşürüldü; `FirmaId` üzerindeki FK/index garantilendi.
3. `dotnet ef database update` çalıştırıldı, snapshot temizlendi.

---

## 2) Bugüne Kadar Push'lanan Commit Sırası
```
9669476  ae23c02  2495cff  4fe88b3  f0fb871  ad4b68f  5d77835
194c99c  1ae42ee  b11b8aa  95c6168  ← bugün
```

---

## 3) Yarın İçin Yapılacaklar (Öncelik Sırasına Göre)

### P1 — Dashboard / Sayfa Doğrulaması
- [ ] Uygulamayı çalıştırıp `Dashboard`, `Servis verileri`, `Finans verileri`, `Belge uyarıları` sayfalarının `42703` hatası olmadan açıldığını doğrula.
- [ ] `PersonelAvansHesap` migration uyarısının kaybolduğunu kontrol et.
- [ ] Tarayıcı konsolu + Visual Studio Output (`Build` ve `Debug` paneli) loglarını kontrol et.

### P2 — Mutabakat Detay Modali Genişletme
**Dosya:** `KOAFiloServis.Web/Components/Pages/Filo/PuantajMutabakat.razor`

Detay modaline iki yeni alt bölüm:
- [ ] **"Eşleşmemiş Faturalar"** — Puantaja bağlı olmayan (`PuantajaBagli == false`) faturalar listesi.
- [ ] **"Faturalanmamış Puantajlar"** — `Faturalandi == false` olan puantaj satırları.
- [ ] Her iki listede **manuel eşleştir** butonu (faturayı puantaj satırına bağla / tersi).
- [ ] Eşleştirme servisi: `IPuantajEslestirmeService.ManuelFaturaPuantajBagla(int puantajId, int faturaId)` (yoksa ekle).
- [ ] Modal alt kısmına "Mutabakat Onayla" CTA (opsiyonel; gelecek aşama).

### P3 — Mutabakat Sayfası Diğer İyileştirmeler
- [ ] Cari/Tedarikçi sekmelerinde **kolon sıralama** (tıklanabilir başlıklar).
- [ ] Tarih aralığı için **hızlı seçenekler**: "Bu Ay", "Geçen Ay", "Bu Hafta".
- [ ] Hesaplama sırasında **per-firma cache** (aynı tarih aralığı tekrar istenirse anında dön).

### P4 — Veri Bütünlüğü Doğrulayıcı
Yeni sayfa: `KOAFiloServis.Web/Components/Pages/Ayarlar/VeriButunluguKontrol.razor`
- [ ] Şu anomalileri tek noktada raporla:
  - `CariId == null` olan aktif Firma sayısı.
  - `CariId == null` olan aktif TasimaTedarikci sayısı.
  - `MuhasebeHesapId == null` olan aktif Cari sayısı.
  - VKN tekrarlanan (duplicate) Cari/Firma/Tedarikçi grupları.
  - `IsDeleted = true` ama hâlâ aktif referansı olan kayıtlar.
- [ ] Her satıra "Düzelt" deeplink (ilgili sayfaya `?id=` parametresiyle).

### P5 — Test / Regression
- [ ] `Solution Test Explorer` üzerinden mevcut testleri çalıştır.
- [ ] `PuantajEslestirmeService` için en az iki birim test:
  - `Firma.CariId` dolu → ID üzerinden eşleşme.
  - `Firma.CariId` boş → Unvan fallback eşleşmesi.

---

## 4) Bilinen Riskler / Notlar
- **Migration sırası**: `20260513140012_FixCariFirmaShadowFK` defansif (IF EXISTS) kullandığı için farklı ortamlarda (DEV/STAGE/PROD) tekrar çalıştırılabilir. Yine de **prod'a almadan önce backup**.
- `EF Snapshot` artık `FirmaId1` içermiyor; sonraki migration'larda yeniden ortaya çıkmaması için `Cari.Firma` explicit konfigürasyonu **silinmemeli**.
- Bulk atama "Tüm Önerileri Ata" akışı VKN/Unvan eşleşmesini varsayar; hatalı eşleşme riskine karşı kullanıcılar **önce küçük bir set** üzerinde denemeli.

---

## 5) Hızlı Komut Hatırlatmaları
```pwsh
# Build
cd C:\Users\muratk\Desktop\d yedek\calisma\Claude-Code\KOAFiloServis\KOAFiloServis.Web
dotnet build -nologo

# DB güncelle
dotnet ef database update

# Yeni migration
dotnet ef migrations add <AdSoyad>

# Son migration'ı geri al (snapshot dahil)
dotnet ef migrations remove

# IIS'e deploy
.\scripts\deploy-iis-local.ps1   # admin PowerShell

# Push
git add -A; git commit -m "..."; git push
```
