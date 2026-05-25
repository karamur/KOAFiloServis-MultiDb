# Puantaj Modülü — Domain Mimarisi

> Son güncelleme: 25.05.2026 — Sprint 3

## Entity İlişkileri (V1)

```
┌──────────────────────┐
│   PuantajHesapDonemi  │  Hesap döngüsü / batch
│   • Yil, Ay, KurumId  │  Unique(FirmaId, Yil, Ay, KurumId, Versiyon)
│   • Versiyon (1,2,3..)│
│   • Durum (Taslak→    │
│     Aktif→Superseded) │
│   • OncekiDonemId (SF)│
└──┬────────┬──────────┘
   │ 1:N    │ 1:N
   ▼        ▼
┌──────────┐  ┌──────────────────────────┐
│PuantajDetay│  │     PuantajKayit         │
│• Op.KaydiId│  │  • HesapDonemiId         │
│• PKayitId  │  │  • OncekiVersiyonId (SF) │
│• HesapDon. │  │  • Versiyon              │
│• Snapshot  │  │  • Gun01..Gun31          │
│  gelir/gider│  │  • Finansal alanlar      │
└────┬───────┘  └──────────────────────────┘
     │ N:1
     ▼
┌──────────────────────┐
│   OperasyonKaydi      │  Saf ham veri
│   • Tarih, Araç,      │  (Islendi/PuantajKayitId KALDIRILDI)
│     Güzergah, Slot    │
└──────────────────────┘
```

## Hesaplama Akışı

```
1. PuantajHesapDonemi oluştur (Taslak)
2. OperasyonKaydi'ları topla (Tarih aralığı)
3. Grupla: GuzergahId + AracId + Slot
4. Her grup → YENİ PuantajKayit (HesapDonemiId + Versiyon)
5. Her OperasyonKaydi → PuantajDetay (snapshot fiyatlarla)
6. Önceki Aktif → Superseded
7. HesapDonemi → Aktif
8. Hata → Rollback (transaction)
```

## Revizyon Zinciri

```
Versiyon 1: PK#100 (Aktif)
    ↓ yeni hesap
Versiyon 2: PK#200 (Aktif), PK#100 (Superseded)
    OncekiVersiyonId=100
```

## Transaction

`BeginTransactionAsync` ile HesapDonemi + PuantajKayit + PuantajDetay tek transaction'da.
Hata → `RollbackAsync`. Partial puantaj oluşmaz.

## Kilitleme

Optimistic concurrency. `HesapDonemi.Aktif` varsa yeni operasyon düzenlemesi engellenir.

## Finansal Audit

PuantajDetay snapshot: hesaplama anındaki BirimGelir/BirimGider dondurulur.
Fiyat sonradan değişse bile iz sürülebilir.

## Servisler

| Servis | Sorumluluk |
|--------|-----------|
| `OperasyonKaydiValidator` | Input validasyon (statik) |
| `OperasyonKaydiBusinessRules` | Domain kuralları + çakışma kontrolü |
| `IOperasyonKaydiService` | CRUD + şablon + migrasyon |
| `IPuantajEngineService` | HesapDonemi + PuantajDetay + revizyon motoru |
| `IKurumPuantajService` | PuantajKayit CRUD (mevcut) |

## Sayfalar

| Route | Sayfa | Açıklama |
|-------|-------|----------|
| `/operasyon-giris` | OperasyonGiris.razor | Günlük operasyon girişi |
| `/kurum-puantaj` | KurumPuantaj.razor | Aylık puantaj grid |
| `/puantaj/import` | KurumPuantajImport.razor | Excel import |
