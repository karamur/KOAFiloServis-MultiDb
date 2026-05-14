# KOA Filo Servis — Kullanım Kılavuzu

**Sürüm:** 1.0  
**Platform:** Web Tarayıcı (Blazor Server / .NET 10)  
**Son Güncelleme:** 2026

---

## İçindekiler

1. [Genel Bakış ve Giriş](#1-genel-bakış-ve-giriş)
2. [Cari Yönetimi](#2-cari-yönetimi)
3. [Filo Yönetimi](#3-filo-yönetimi)
4. [Personel Yönetimi](#4-personel-yönetimi)
5. [Muhasebe](#5-muhasebe)
6. [Bütçe ve Finans](#6-bütçe-ve-finans)
7. [Analiz ve Raporlar](#7-analiz-ve-raporlar)
8. [Faturalama](#8-faturalama)
9. [Stok Yönetimi](#9-stok-yönetimi)
10. [Ayarlar ve Sistem](#10-ayarlar-ve-sistem)

---

## 1. Genel Bakış ve Giriş

KOA Filo Servis; filo yönetimi, cari takip, personel finans, muhasebe ve analiz işlemlerini tek platformda birleştiren entegre bir yönetim sistemidir.

### 1.1 Sisteme Giriş

1. Tarayıcıda uygulama adresini açın.
2. **Kullanıcı adı** ve **şifrenizi** girin.
3. Eğer lisans kurulumu yapılmamışsa sistem sizi **Kurulum Sihirbazı**'na yönlendirir.

### 1.2 Ana Panel (Dashboard)

Giriş yaptıktan sonra karşılama ekranında şu bilgiler görüntülenir:

| Bölüm | Açıklama |
|-------|----------|
| Araç Sayısı | Aktif filo büyüklüğü |
| Açık Faturalar | Bekleyen tahsilat/tediye |
| Avans / Borç | Personel finansal özeti |
| Uyarılar | Belge süresi dolan araçlar, gecikmiş ödemeler |

### 1.3 Gezinme

- Sol menüden modüller arasında geçiş yapılır.
- Her modülün kendi alt menüleri vardır.
- Üst sağda **firma seçici** bulunur; çok firmalı kullanımda firmalar arası geçiş buradan yapılır.

---

## 2. Cari Yönetimi

**Menü Yolu:** Cariler

### 2.1 Cari Listesi

`Cariler → Cari Listesi` ekranında tüm müşteri ve tedarikçi kayıtları listelenir.

**Filtreleme:**
- Cari adı, kodu veya vergi numarasına göre arama yapılabilir.
- Cari türüne göre (Müşteri / Tedarikçi / Her İkisi) filtre uygulanabilir.
- Aktif/Pasif durumuna göre filtreleme yapılabilir.

**Yeni Cari Eklemek:**
1. Sağ üstteki **"+ Yeni Cari"** butonuna tıklayın.
2. Formu doldurun:
   - **Unvan / Ad Soyad** (zorunlu)
   - **Cari Kodu** (otomatik üretilir, değiştirilebilir)
   - **Vergi No / TC Kimlik No**
   - **Adres, Telefon, E-posta**
   - **Cari Türü:** Müşteri, Tedarikçi veya her ikisi
   - **Ödeme Vadesi** (gün)
   - **Kredi Limiti**
3. **Kaydet** butonuna tıklayın.

### 2.2 Cari Hareket Takibi

`Cariler → Cari Hareket Takip` ekranından:
- Seçilen cari için tüm fatura, tahsilat ve tediye hareketleri görüntülenir.
- Tarih aralığı filtresi uygulanabilir.
- **Bakiye, Borç, Alacak** özetleri anlık gösterilir.
- Excel'e aktarma butonu mevcuttur.

### 2.3 Cari Risk Analizi

`Cariler → Cari Risk Analizi` ekranında:
- Vadesi geçmiş bakiyeler renkli gösterilir.
- Risk skoru (düşük / orta / yüksek) otomatik hesaplanır.
- Yaşlandırma raporu (0-30, 31-60, 61-90, 90+ gün) görüntülenebilir.

### 2.4 Cari Yaşlandırma Raporu

`Raporlar → Cari Yaşlandırma Raporu` sayfasından:
1. Tarih aralığı ve cari türü seçin.
2. **Raporu Göster** butonuna tıklayın.
3. Excel veya PDF olarak indirebilirsiniz.

---

## 3. Filo Yönetimi

**Menü Yolu:** Araclar / AracTakip / Filo / FiloOperasyon

### 3.1 Araç Listesi

`Araclar → Araç Listesi` ekranında tüm araçlar listelenir.

**Araç Bilgileri:**
- Plaka, marka, model, yıl
- Araç tipi (Kendi Malı / Kiralık)
- Aktif sürücü ataması
- Sigorta, muayene, egzoz muayenesi son tarihleri
- Kilometre bilgisi

**Yeni Araç Eklemek:**
1. **"+ Yeni Araç"** butonuna tıklayın.
2. Temel bilgileri girin (plaka, marka, model, yıl, şase no, motor no).
3. Belge tarihlerini girin (ruhsat, sigorta, muayene).
4. Sürücü ataması yapın.
5. **Kaydet**'e tıklayın.

### 3.2 Araç Takip

`AracTakip` modülünden:
- Araç konum ve güzergah bilgileri takip edilir.
- Yakıt tüketim girişleri yapılır.
- Servis kayıtları eklenir.

### 3.3 Araç Masrafları

`AracMasraflari` modülünden:
- Yakıt, bakım, sigorta, vergi gibi masraflar kategorilere göre kaydedilir.
- **Personel cebinden ödeme** seçeneği: Sürücünün kendi parasıyla ödediği masraflar işaretlenerek ileride personele geri ödenebilir.
- Masraf türüne göre filtreleme ve toplam görünümü mevcuttur.

### 3.4 Belge Uyarıları

`Belge Uyarıları` sayfasında:
- Süresi yaklaşan veya dolmuş belgeler (sigorta, muayene, ehliyet) listelenir.
- Renk kodlaması: 🟢 Geçerli / 🟡 Yaklaşıyor (30 gün) / 🔴 Dolmuş
- SMS veya e-posta bildirimi ayarlanabilir.

### 3.5 Filo Operasyonu

`FiloOperasyon` modülünden:
- Araç atama ve görev takibi yapılır.
- Güzergah planlaması yapılır.
- Servis çalışma kayıtları girilir.

### 3.6 Kiralık Araç Raporu

`Raporlar → Kiralık Araç Raporu` sayfasından kiralık araçlara ait maliyet ve kullanım analizi görüntülenir.

### 3.7 Lastik Yönetimi

`Lastik` modülünden:
- Araçlara lastik ataması yapılır.
- Lastik değişim tarihleri ve km bilgisi kaydedilir.
- Stok takibi yapılır.

---

## 4. Personel Yönetimi

**Menü Yolu:** Soforler / Personel

### 4.1 Personel (Sürücü) Listesi

`Soforler → Sürücü Listesi` ekranından:
- Tüm sürücü/personel kayıtları listelenir.
- Aktif/Pasif, göreve göre filtreleme yapılabilir.

**Yeni Personel Eklemek:**
1. **"+ Yeni Sürücü"** butonuna tıklayın.
2. Kimlik bilgileri, iletişim bilgileri ve istihdam bilgilerini doldurun.
3. Ehliyet bilgileri ve belge sürelerini girin.
4. Maaş bilgilerini girin.
5. **Kaydet**'e tıklayın.

### 4.2 Maaş / Ödeme Yönetimi

**Menü Yolu:** `Personel → Maaş / Ödeme Yönetimi`

Bu ekran personel ödeme sürecinin merkezi yönetim noktasıdır.

#### 4.2.1 Durum Tablosu

Tüm aktif personel listelenir. Her satırda:
- Gerçek Maaş, Bankaya Yatan, Avans, Harcama, Ödenecek tutarlar gösterilir.
- Personele tıklandığında sağda detay paneli açılır.

#### 4.2.2 Personel Detay Sekmeler

Personel seçildiğinde sağ panelde şu sekmeler bulunur:

| Sekme | İçerik |
|-------|--------|
| **Ekstra** | Finansal özet, açık avans, kalan borç, cebinden harcama, ekstre |
| **Avanslar** | Tüm avans kayıtları, yeni avans ekleme |
| **Borçlar** | Borç kayıtları, ödeme yapma |
| **Maaş Geçmişi** | Geçmiş dönem maaş kayıtları |
| **Harcamalar** | Personelin cebinden yaptığı harcamalar |
| **Hakediş Pusulası** | Yazdırılabilir ödeme pusulası |

#### 4.2.3 Yeni Avans Eklemek

1. Personeli seçin → **Avanslar** sekmesine geçin.
2. **"+ Yeni Avans"** butonuna tıklayın.
3. Tarih, tutar, açıklama ve ödeme şeklini girin.
4. **Kaydet**'e tıklayın.
5. Avans kaydedilince otomatik muhasebe fişi oluşturulur.

#### 4.2.4 Avans Mahsubu

Açık avanstan maaşa mahsup işlemi:
1. Maaş düzenleme modunda **"Açık Avans Mahsubu"** bölümüne bakın.
2. Açık avanslar listelenir; **"Maaşa Mahsup Et"** butonuna tıklayın.
3. Mahsup tutarı otomatik hesaplanarak maaş avans alanına işlenir.
4. Otomatik mahsup muhasebe fişi oluşturulur.

#### 4.2.5 Borç Kaydı ve Ödeme

**Borç Eklemek:**
1. **Borçlar** sekmesine geçin.
2. Borç tarihini, tutarını, nedenini ve borç tipini girin.
3. Kaydedin — otomatik muhasebe fişi oluşturulur.

**Borç Ödemek:**
1. İlgili borç satırında **💰 ödeme** ikonuna tıklayın.
2. Ödeme tarihini, tutarını ve ödeme şeklini seçin.
3. **Kaydet**'e tıklayın — otomatik muhasebe fişi oluşturulur.

#### 4.2.6 Maaş Ödeme İşlemi

1. Personel satırında **"Ödeme Yap"** butonuna tıklayın.
2. Ödeme yöntemini seçin: **Elden / Banka / Maaştan Mahsup / Kredi Kartı**
3. Tarih ve tutar onaylayın.
4. **"Ödemeyi Kaydet"** butonuna tıklayın.

### 4.3 İzin / Devamsızlık

`Personel → İzin Yönetimi` modülünden izin talepleri ve devamsızlık kayıtları tutulur.

### 4.4 Hakediş / Pusula Yazdırma

Personel detay panelindeki **Hakediş Pusulası** sekmesinden:
- Aylık ödeme özeti görüntülenir.
- Yazdır butonu ile yazdırılabilir formata aktarılır.

---

## 5. Muhasebe

**Menü Yolu:** Muhasebe

### 5.1 Hesap Planı

`Muhasebe → Hesap Planı` ekranında:
- Tekdüzen muhasebe hesap planı (1-8 sınıf hesaplar) görüntülenir.
- Yeni hesap eklenebilir, var olanlar düzenlenebilir.
- **Excel'den içe aktarma** özelliği ile toplu hesap yüklemesi yapılabilir.

### 5.2 Muhasebe Fişleri

`Muhasebe → Fişler` ekranında:

**Fiş Türleri:**

| Tür | Kısaltma | Açıklama |
|-----|----------|----------|
| Mahsup | MH | Genel muhasebe kaydı |
| Tahsilat | TH | Para giriş işlemleri |
| Tediye | TD | Para çıkış işlemleri |
| Açılış | AC | Dönem başı açılış |
| Kapanış | KP | Dönem sonu kapanış |

**Yeni Fiş Oluşturmak:**
1. **"+ Yeni Fiş"** butonuna tıklayın.
2. Fiş türünü seçin.
3. Fiş tarihini girin (FisNo otomatik üretilir).
4. Kalem ekleyin: Hesap kodu, Borç/Alacak tutarı, açıklama.
5. Borç = Alacak kontrolü sistem tarafından yapılır.
6. **"Kaydet"** ardından **"Onayla"** butonuna tıklayın.

> ⚠️ **Önemli:** Onaylanmış fişler düzenlenemez ve silinemez.

**Otomatik Fişler:**
Şu işlemler gerçekleştiğinde sistem otomatik muhasebe fişi oluşturur:
- Fatura kesildiğinde
- Personel avansı verildiğinde
- Avans mahsubu yapıldığında
- Borç kaydedildiğinde
- Borç ödendiğinde
- Banka/Kasa hareketi girildiğinde

### 5.3 Banka / Kasa Hesapları

`BankaHesaplari` modülünden:
- Şirketin banka hesapları tanımlanır.
- Her hesap için bakiye takibi yapılır.

`BankaHareketleri` modülünden:
- Giriş/çıkış hareketleri kaydedilir.
- Fatura ile eşleştirme yapılabilir.
- Otomatik muhasebe fişi oluşturulur.

### 5.4 Raporlar

#### Yevmiye Defteri
`Muhasebe → Yevmiye` sayfasından:
1. Başlangıç ve bitiş tarihi seçin.
2. **"Raporu Göster"** butonuna tıklayın.
3. Excel veya Zirve formatında export alabilirsiniz.

#### Mizan
`Muhasebe → Mizan` sayfasından tüm hesapların borç/alacak dengesi görüntülenir.

#### Muavin (Hesap Defteri)
`Muhasebe → Muavin` sayfasından belirli bir hesabın detaylı hareket listesi görüntülenir.

#### Bilanço
`Muhasebe → Bilanço` sayfasından aktif/pasif bilanço tablosu görüntülenir.

#### Gelir/Gider Tablosu
`Muhasebe → Gelir/Gider` sayfasından aylık veya yıllık gelir/gider analizi görüntülenir.

#### KDV Beyanname
`Muhasebe → KDV Beyanname` sayfasından aylık KDV özeti görüntülenir.

### 5.5 Dönem Yönetimi

- Muhasebe dönemleri (12 aylık) sistem tarafından yönetilir.
- Dönem kapatıldığında o döneme ait kayıtlar kilitlenir.
- Aktif dönem dışında işlem yapılamaz.

### 5.6 Zirve Export

`Muhasebe → Zirve Export` sayfasından Zirve muhasebe programı formatında XML/Excel export alınabilir.

---

## 6. Bütçe ve Finans

**Menü Yolu:** Butce / Budget / Finans

### 6.1 Bütçe Planlama

`Butce → Aylık Ödemeler` sayfasından:
- Aylık sabit ödemeler (kira, sigorta, leasing taksitleri vb.) planlanır.
- Takvim görünümünde ödeme takvimi izlenebilir.

`Butce → Hedef / Gerçekleşen` sayfasından:
- Planlanan bütçe ile gerçekleşen değerler karşılaştırılır.
- Sapma analizi yapılır.

### 6.2 Kredi / Leasing Yönetimi

`Budget → Kredi Raporu` sayfasından:
- Aktif krediler ve leasing sözleşmeleri listelenir.
- Taksit planı görüntülenir.

`Budget → Kredi Taksitleri` sayfasından:
- Aylık taksit takibi yapılır.
- Ödenen / ödenecek taksitler işaretlenir.

### 6.3 Risk Analizi

`Budget → Risk Analizi` sayfasından:
- Nakit akış riski değerlendirilir.
- Gelecek 30-90 günlük ödeme yükümlülükleri hesaplanır.
- Rapor Excel'e aktarılabilir.

### 6.4 Ödeme Yönetimi

`Budget → Ödeme Yönetimi` sayfasından:
- Tüm ödemeler tek ekranda görüntülenir.
- Vadesi geçmiş ödemeler kırmızı ile işaretlenir.
- Toplu ödeme işlemi yapılabilir.

### 6.5 Ödeme Eşleştirme

`OdemeEslestirme` modülünden:
- Banka ekstresindeki ödemeler faturalarla eşleştirilir.
- Eşleştirilmemiş hareketler listelenir.

---

## 7. Analiz ve Raporlar

**Menü Yolu:** Raporlar

### 7.1 Mali Analiz Dashboard

`Raporlar → Mali Analiz Dashboard` sayfasında:
- Gelir/Gider özeti grafik olarak gösterilir.
- Nakit akış tablosu görüntülenir.
- Kâr/Zarar özeti aylık bazda izlenir.
- En yüksek maliyetli araçlar listelenir.

### 7.2 Sürücü Performans Raporu

`Raporlar → Sürücü Performans` sayfasında:
- Sürücü bazında yakıt tüketimi, mesafe, masraf karşılaştırması yapılır.
- En verimli / en az verimli sürücüler listelenir.

### 7.3 Yakıt Verimlilik Raporu

`Raporlar → Yakıt Verimlilik` sayfasında:
- Araç bazında lt/100km hesaplanır.
- Dönemler arası karşılaştırma yapılır.
- Normalin dışındaki araçlar işaretlenir.

### 7.4 Fatura Ödeme Raporu

`Raporlar → Fatura Ödeme Raporu` sayfasında:
- Ödenen / bekleyen / gecikmiş faturalar listelenir.
- Cari bazında filtreleme yapılabilir.
- Excel çıktısı alınabilir.

### 7.5 Servis Çalışma Raporu

`Raporlar → Servis Çalışma Raporu` sayfasında:
- Servis personelinin aylık çalışma süreleri listelenir.
- Puantaj tablosu görüntülenir.

### 7.6 Komisyon Raporu

`Raporlar → Komisyon Raporu` sayfasında satış personelinin komisyon hesaplaması yapılır.

### 7.7 Özmal / Kiralık Araç Karşılaştırması

`Raporlar → Özmal Araç` ve `Raporlar → Kiralık Araç` sayfalarından maliyet karşılaştırması yapılır.

---

## 8. Faturalama

**Menü Yolu:** Faturalar / EFatura / ProformaFaturalar

### 8.1 Fatura Listesi

`Faturalar` modülünden:
- Tüm satış ve alış faturaları listelenir.
- Fatura no, tarih, cari, tutar ve ödeme durumuna göre filtreleme yapılabilir.

**Fatura Durumları:**
- 🟡 Taslak
- 🔵 Onaylandı
- 🟢 Ödendi
- 🔴 Gecikmiş
- ⚫ İptal

### 8.2 Yeni Fatura Oluşturmak

1. **"+ Yeni Fatura"** butonuna tıklayın.
2. Fatura türünü seçin (Satış / Alış).
3. Cari seçin.
4. Fatura kalemlerini ekleyin (stok/hizmet, miktar, birim fiyat, KDV oranı).
5. Toplam, KDV ve genel toplam otomatik hesaplanır.
6. **"Kaydet"** veya **"Onayla"** butonuna tıklayın.

### 8.3 e-Fatura

`EFatura` modülünden:
- e-Fatura mükellefleri için elektronik fatura kesilebilir.
- GİB entegrasyonu ayarlardan yapılandırılır.
- Fatura durumu (gönderildi / kabul edildi / reddedildi) takip edilir.

### 8.4 Proforma Fatura

`ProformaFaturalar` modülünden teklif/proforma fatura hazırlanabilir.

### 8.5 Fatura Hazırlık

`Raporlar → Fatura Hazırlık` sayfasından toplu fatura kesimine hazırlık yapılabilir.

### 8.6 Toplu Fatura Muhasebeleştirme

`Muhasebe → Toplu Muhasebeleştirme` sayfasından:
- Muhasebeleştirilmemiş faturalar listelenir.
- Toplu seçimle tek seferde muhasebe kaydı oluşturulabilir.

### 8.7 Servis Operasyon Puantaj ve Fatura (Resimli)

Servis Operasyon kontrat tabanlı puantaj/fatura akışı için detaylı ve adım adım resimli kılavuz aşağıdadır:

- Geliştirici dokümanı: `KOAFiloServis.Web/Docs/puantaj-ve-fatura-kilavuzu.md`
- Yayınlanan doküman: `KOAFiloServis.Web/wwwroot/docs/puantaj-ve-fatura-kilavuzu.md`

Bu kılavuzda özellikle şu başlıklar resimli anlatılmıştır:

1. Kurum seçimi ve `+` ile hızlı kurum kaydı
2. Kuruma göre güzergah kodu önerisi ve manuel düzenleme
3. Plakadan personel ve telefon otomatik doldurma
4. Yön bilgisi, gelir/gider fiyatı ve firma tip ayrımı
5. Aylık puantajdan fatura kontrolüne kadar uçtan uca akış

---

## 9. Stok Yönetimi

**Menü Yolu:** Stok

### 9.1 Stok Kartları

`Stok → Stok Kartları` sayfasından:
- Yedek parça, lastik, sarf malzeme gibi stok kalemleri tanımlanır.
- Birim, kategori, min. stok seviyesi, fiyat bilgileri girilir.

### 9.2 Stok Hareketleri

`Stok → Stok Hareketler` sayfasından:
- Giriş (alım), çıkış (kullanım/satış) ve iade hareketleri kaydedilir.
- Her hareket için araç veya iş emri ile ilişkilendirme yapılabilir.

### 9.3 Stok İşlemleri

`Stok → Stok İşlemleri` sayfasından:
- Toplu giriş/çıkış işlemleri yapılabilir.
- Sayım farkı düzeltme işlemi yapılabilir.

### 9.4 Araç İşlem

`Stok → Araç İşlem` sayfasından araçlara yönelik servis ve parça işlemleri kaydedilir.

---

## 10. Ayarlar ve Sistem

**Menü Yolu:** Ayarlar

### 10.1 Şirket Yönetimi

`Ayarlar → Şirket Yönetimi` sayfasından:
- Şirket bilgileri (unvan, adres, vergi no, logo) düzenlenir.
- Çok şirket kullanımında yeni şirket eklenebilir.

### 10.2 Kullanıcı ve Rol Yönetimi

`Ayarlar → Rol Yönetimi` sayfasından:
- Kullanıcı rolleri tanımlanır (Yönetici, Muhasebe, Operasyon vb.).
- Her role sayfa bazında yetki atanır.

### 10.3 Personel Finans Ayarları

`Ayarlar → Personel Finans Ayarları` sayfasından:
- Avans verme, mahsup ve borç ödemelerinde kullanılacak muhasebe hesapları seçilir.
- Otomatik fiş oluşturma açılıp kapatılabilir.

### 10.4 Yedekleme

`Ayarlar → Yedekleme` sayfasından:
- Veritabanı yedeği alınabilir.
- Otomatik yedekleme zamanlaması yapılabilir.

### 10.5 SMS Ayarları

`Ayarlar → SMS Ayarları` sayfasından SMS gönderim entegrasyonu yapılandırılır.

### 10.6 Sistem Sağlığı

`Ayarlar → Sistem Sağlığı` sayfasından sunucu ve veritabanı durumu izlenebilir.

### 10.7 Güncelleme

`Ayarlar → Uygulama Güncelleme` sayfasından sistem güncellemesi yapılabilir.

---

## Sık Sorulan Sorular (SSS)

**S: Muhasebe fişi yanlış oluşturuldu, nasıl düzeltirim?**  
C: Taslak durumdaki fişler düzenlenebilir. Onaylanmış fişler için iptal fişi (ters kayıt) oluşturulmalıdır.

**S: Personel avansını silerken hata alıyorum.**  
C: Mahsuplaşması olan avanslar silinemez. Önce mahsup kayıtlarını silin.

**S: Otomatik muhasebe kaydı oluşturulmuyor.**  
C: `Ayarlar → Personel Finans Ayarları` sayfasında hesap kodlarını ve "Otomatik Fiş Oluştur" seçeneğini kontrol edin.

**S: Excel çıktısı boş geliyor.**  
C: Filtreleme kriterlerini kontrol edin; tarih aralığı ve seçili dönemde veri olduğundan emin olun.

**S: Belge uyarısı bildirimleri gelmiyor.**  
C: `Ayarlar → SMS Ayarları` veya e-posta bildirim ayarlarını kontrol edin.

---

## Kısayollar ve İpuçları

| İşlem | Açıklama |
|-------|----------|
| `Ctrl + F` | Sayfada arama |
| Sağ tık menüsü | Listede hızlı işlem |
| Excel butonu | Her listede dışa aktarım |
| Yenile butonu 🔄 | Anlık veri güncelleme |
| Filtre temizle | Tüm filtreleri sıfırla |

---

*Bu kılavuz KOA Filo Servis uygulamasının güncel sürümü baz alınarak hazırlanmıştır.*
