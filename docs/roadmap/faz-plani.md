# Operasyon & Puantaj Modülü — Sprint Planı

> Son güncelleme: 25.05.2026 — Sprint 3 tamamlandı

## Tamamlanan Sprintler

### Sprint 1: OperasyonKaydi Entity Mimarisi ✅

- OperasyonKaydi entity (39 alan, IFirmaTenant, audit, soft delete)
- 8 FK Restrict, 13 performans indexi
- OperasyonKaydiValidator + BusinessRules + Service (3 katmanlı)
- PuantajEngineService (ilk versiyon)
- EF Migration: OperasyonKayitlari tablosu

### Sprint 2: Operasyon Giriş Ekranı ✅

- `/operasyon-giris` sayfası: tarih/kurum/güzergah filtresi
- Grid: inline edit (slot/sefer/durum), dirty tracking, toplu kaydet
- Yeni kayıt: inline form (araç/şoför autocomplete, slot toggle)

### Sprint 3: Puantaj Engine V1 ✅

- PuantajHesapDonemi: hesap döngüsü (Unique: FirmaId+Yil+Ay+KurumId+Versiyon)
- PuantajDetay: Operasyon↔Puantaj bağlantısı + snapshot (finansal audit)
- PuantajKayit: HesapDonemiId + OncekiVersiyonId (Self-FK revizyon zinciri) + Versiyon
- OperasyonKaydi sadeleştirme: Islendi/IslenmeTarihi/PuantajKayitId kaldırıldı
- Transaction scope: BeginTransactionAsync + CommitAsync/RollbackAsync
- Revizyon: yeni hesap → önceki Superseded, self-FK zinciri
- Migration: 2 yeni tablo + 3 kolon sil

---

## Yapılacak Sprintler

### Sprint 4: Puantaj Hesap UI + Tetikleme 🟡

| İş | Öncelik |
|----|:---:|
| PuantajEngine tetikleme butonu (UI'da "Hesapla") | 🔴 |
| Hesap dönemi listesi + durum badge'leri | 🔴 |
| PuantajDetay görüntüleme (hangi operasyon hangi PK'ya gitti) | 🔴 |
| İptal etme butonu | 🟡 |
| OperasyonKaydi Excel import sayfası | 🟡 |

### Sprint 5: Raporlama + Dashboard ⚪

| İş | Öncelik |
|----|:---:|
| Versiyon karşılaştırma (V1 vs V2 fark tablosu) | ⚪ |
| Günlük operasyon özet kartı (Dashboard) | ⚪ |
| Eksik/hatalı operasyon raporu | ⚪ |
| PuantajEngine Quartz job (ay sonu otomatik) | ⚪ |

---

## Servis Envanteri

| Servis | Tip | Dosya |
|--------|-----|-------|
| OperasyonKaydiValidator | static | `Services/OperasyonKaydiValidator.cs` |
| OperasyonKaydiBusinessRules | scoped | `Services/OperasyonKaydiBusinessRules.cs` |
| IOperasyonKaydiService | scoped | `Services/Interfaces/IOperasyonKaydiService.cs` |
| IPuantajEngineService | scoped | `Services/Interfaces/IPuantajEngineService.cs` |
| PuantajEngineService | scoped | `Services/PuantajEngineService.cs` |

## Entity Envanteri

| Entity | Tablo | Açıklama |
|--------|-------|----------|
| OperasyonKaydi | OperasyonKayitlari | Günlük ham operasyon (saf veri) |
| PuantajHesapDonemi | PuantajHesapDonemleri | Hesap döngüsü + revizyon |
| PuantajDetay | PuantajDetaylari | Operasyon↔Puantaj bağlantısı + snapshot |
| PuantajKayit | PuantajKayitlar | Aylık hesaplanmış çıktı |

## Sayfa Envanteri

| Route | Sayfa | Yetki |
|-------|-------|-------|
| `/operasyon-giris` | OperasyonGiris.razor | Admin, Operasyon, Muhasebeci, HoldingYoneticisi |
| `/kurum-puantaj` | KurumPuantaj.razor | Admin, Operasyon, Muhasebeci, HoldingYoneticisi |
| `/puantaj/import` | KurumPuantajImport.razor | Admin, Operasyon |
