# Operasyon & Puantaj Modülü — Sprint Planı

> Son güncelleme: 25.05.2026 — 7 Sprint tamamlandı

## Tamamlanan Sprintler (7/7)

| Sprint | Konu | Commit | Test |
|:------:|------|--------|:----:|
| S1 | OperasyonKaydi Entity Mimarisi | `1554c98` | 305 |
| S2 | Operasyon Giriş Ekranı | `1554c98` | 305 |
| S3 | Puantaj Engine V1 — Revizyon + Detay + Audit | `87f8988` | 305 |
| S4 | Puantaj Hesaplama UI + Preview + Comparison + Drill-down | `7031b1a` | 305 |
| S5 | Workflow + Approval + Authorization + Audit Log | `db8c9e7` | 305 |
| S6 | Finansal Çıktı — Fatura + Snapshot Köprüsü | `bc7594d` | 305 |
| S7 | OperasyonKaydi Excel Import + Toplu Giriş + ADR | `78236a8` | 305 |

## Entity Envanteri

| Entity | Tablo | Sprint |
|--------|-------|:------:|
| OperasyonKaydi | OperasyonKayitlari | S1 |
| PuantajHesapDonemi | PuantajHesapDonemleri | S3 |
| PuantajDetay | PuantajDetaylari | S3 |
| PuantajKayit | PuantajKayitlar (mevcut, güncellendi) | S1, S3 |
| PuantajAuditLog | PuantajAuditLogs | S5 |
| PuantajFinansalKayit | PuantajFinansalKayitlar | S6 |

## Servis Envanteri

| Servis | Sorumluluk | Sprint |
|--------|-----------|:------:|
| OperasyonKaydiValidator | Input validasyon (statik) | S1 |
| OperasyonKaydiBusinessRules | Domain kuralları + çakışma + kilit kontrolü | S1, S5 |
| IOperasyonKaydiService | CRUD + şablon + Excel import | S1, S7 |
| IPuantajEngineService | HesapDonemi + PuantajDetay + revizyon | S3 |
| IPreviewEngineService | Dry-run + comparison + drill-down | S4 |
| IPuantajWorkflowService | Onay zinciri (Finans→Muhasebe→Kilit) | S5 |
| IPuantajFinansService | Finansal kayıt + fatura üretimi | S6 |

## Sayfa Envanteri

| Route | Sayfa | Sprint |
|-------|-------|:------:|
| `/operasyon-giris` | OperasyonGiris.razor | S2, S7 |
| `/operasyon/import` | OperasyonImport.razor | S7 |
| `/puantaj-hesaplama` | PuantajHesaplama.razor | S4, S5, S6 |
| `/kurum-puantaj` | KurumPuantaj.razor | Mevcut |
| `/puantaj/import` | KurumPuantajImport.razor | Mevcut |

## ADR Envanteri

| ADR | Başlık |
|-----|--------|
| 0001 | OperasyonKaydi + PuantajKayit İki Katmanlı Mimari |
| 0002 | PuantajEngine V1 — Revizyon + Detay + Audit |
| 0003 | Workflow + Approval State Machine |
| 0004 | Finansal Çıktı — Fatura + Snapshot Köprüsü |
| 0005 | Multi-Tenant Database-Per-Firma Mimarisi |

## Sonraki Sprint Önerileri

| Özellik | Öncelik |
|---------|:---:|
| Engine Quartz Job (aylık otomatik hesaplama) | Yüksek |
| Holding Konsolidasyon (finansal read model) | Yüksek |
| Dashboard KPI (operasyon/puantaj özet kartları) | Orta |
| PuantajKayit Excel Import → OperasyonKaydi geçiş | Orta |
| E-fatura entegrasyonu (GIB) | Düşük |
