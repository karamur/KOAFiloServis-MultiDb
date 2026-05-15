# Puantaj Modülü İlerleme Notları

Son güncelleme: 2026-05-15

## ✅ Yapılanlar

### Veri Modeli / Migration
- `Guzergah.KurumId` eklendi; Kurum ile Cari ayrı kavramlar olarak tutuluyor (Kurum'un Cari'ye bağlı olma zorunluluğu kaldırıldı).
- Eksik olan `Guzergahlar.KurumId` kolonu için idempotent migration uygulandı: `20260515010739_EnsureGuzergahKurumIdColumn`.
- Puantaj onay kolonları (`Onaylandi`, `OnayTarihi`) için migration'lar eklendi:
  - `20260514234625_AddPuantajOnaylandi`
  - `20260515000133_AddPuantajOnayKolonlari`
- `ApplicationDbContextModelSnapshot.cs` güncel.

### Servisler
- `GuzergahService` artık `Kurum` include ediyor ve `KurumId` alanını kaydediyor.

### Güzergah Formu
- `GuzergahForm.razor` Kurum/Müşteri Kartı seçimi ile Cari seçimini birbirinden bağımsız hâle getirdi.

### Puantaj Sayfası (`FiloGunlukPuantajPage.razor`)
- Toplu puantaj akışı:
  - Kurum / Müşteri Kartı autocomplete ile seçiliyor.
  - Seçili kuruma bağlı güzergahlar `KurumId / CariId / FirmaId` fallback zinciri ile bulunuyor.
  - Eşleştirme yoksa varsayılan araç/şoför bulunan güzergahlar için kullanıcı onayıyla otomatik (geçici) eşleştirme türetiliyor.
  - Kuruma bağlı `Firma` yoksa Kurum bilgilerinden `CariId = null` ile otomatik Firma kaydı oluşturuluyor (kullanıcı onayı ile).
  - "Yeniden Hesapla" akışı: onaylı / faturalanmış / ödenmiş kayıtları korur, diğerlerini siler ve yeniden üretir.
  - Kaydedilmemiş eşleştirmeler için `FiloGuzergahEslestirmeId = null` gönderiliyor (FK hatası giderildi).
  - Hata mesajlarında `InnerException` detayı da gösteriliyor.
- Hiyerarşik liste UI: Güzergah → Araç → Günlük satır (açılır/kapanır).
- Satır içi (inline) hızlı düzenleme: tarih, servis türü, sefer, şoför, gelir, gider/maliyet, durum.
- Hızlı kaydet / vazgeç (orijinal değerlere geri dönme).
- Filtre alanına **Ay** seçici eklendi (`<input type="month">`); seçim yapılınca `filtreBaslangic` ve `filtreBitis` o ayın 1'i ile son günü olarak ayarlanıyor.
- Güzergah grup başlığında onaysız / onaylı kayıt rozetleri ve **Toplu Onayla** butonu.
- `GuzergahTopluOnayla(int guzergahId)` metodu: ilgili güzergahta yalnızca `!Onaylandi` olan kayıtları DB'den tekrar filtreleyip toplu onaylıyor (yarış durumlarına karşı), `OnayTarihi = DateTime.Now` set ediyor, sonra `LoadAsync()` çağırıyor.

### Genel
- Build başarılı (en son toplu onay özelliği sonrası doğrulandı).

## 🟡 Devam Eden / Yarın İçin TODO

### Toplu Onay İyileştirmeleri
- [ ] Güzergah grubunda araç bazlı (Araç satırında) "Toplu Onayla" butonu da ekleyelim mi? — kullanıcı kararı bekliyor.
- [ ] Tüm listeyi (tüm güzergahlar) tek tıkla onaylayan üst-seviye "Hepsini Onayla" butonu (filtre kapsamında).
- [ ] Toplu onay sonrası özet toast'a "atlanan / hatalı kayıt" sayıları (şu an sadece başarılı sayı gösteriliyor).
- [ ] Toplu **onay kaldırma** karşılığı (faturalanmamış / ödenmemiş kayıtlar için).

### Faturalama / Ödeme Akışı
- [ ] Onaylı kayıtlardan kurum faturasına aktarma akışı (ekran + servis).
- [ ] Tedarikçi ödeme dekontu / fatura akışı.
- [ ] `KurumFaturaKesildiMi` / `TaseronOdemeYapildiMi` set eden ekranlar.

### Raporlama
- [ ] Aylık özet rapor: kurum bazında gelir / gider / sefer.
- [ ] Araç bazında günlük doluluk raporu.
- [ ] Excel / PDF export (mevcut filtre + hiyerarşi ile).

### Veri Tutarlılığı
- [ ] Kurum ↔ Firma manuel eşleştirme ekranı (otomatik üretilen Firma kayıtlarını sonradan birleştirebilmek için).
- [ ] Aynı (Tarih, Araç, Servis Türü) için duplicate puantaj önleme (DB unique index önerisi).

### UX Küçük İyileştirmeler
- [ ] Açık olan güzergahların durumunu sayfa yenilemede koruma (state).
- [ ] Hızlı düzenleme sırasında klavye kısayolları (Enter = kaydet, Esc = iptal).
- [ ] Toplu Onayla butonuna spinner / disabled state (`isSaving` zaten var ama buton özelinde gösterge eklenebilir).

### Test / Doğrulama
- [ ] Toplu onay senaryolarını manuel test et:
  - Hiç onaysız kayıt yokken buton davranışı.
  - Filtre ile sınırlandırılmış listede sadece görünenleri onayladığını doğrula (şu an `kayitlar` üzerinden çalışıyor → filtre kapsamı = onay kapsamı).
  - Yarış durumu: aynı anda iki kullanıcı onaylarsa.
- [ ] Birim test: `GuzergahTopluOnayla` için integration test.

## 📌 Notlar
- `kayitlar` listesi mevcut filtre sonucunu tutuyor; `GuzergahTopluOnayla` bu listeden ID toplayıp DB'den tekrar filtreleyerek onayladığı için **filtre dışı kayıtlar etkilenmez**. Bu davranış kullanıcı beklentisine uygun olmalı, doğrulanacak.
- Kurum ve Cari **ayrı** tutulmaya devam ediyor; otomatik Firma oluşturmada `CariId = null` bırakılıyor.
