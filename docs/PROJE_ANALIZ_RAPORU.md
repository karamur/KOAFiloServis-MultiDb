# MKFiloServis MultiDb Proje Analiz Raporu

**Rapor tarihi:** 2 Ağustos 2026  
**İncelenen solution:** `MKFiloServis.slnx`  
**İnceleme yöntemi:** Salt-okunur kaynak analizi ve solution build'i  
**Build sonucu:** Başarılı — 0 hata, 2 uyarı

## 1. Yönetici Özeti

MKFiloServis MultiDb; filo, personel, puantaj, hakediş, finans, muhasebe, CRM, EBYS, stok ve servis operasyonlarını aynı uygulama çatısı altında toplayan, ağırlıklı olarak .NET 10 ve Blazor Interactive Server kullanan geniş kapsamlı bir iş uygulamasıdır.

Solution beş aktif projeden oluşmaktadır. İş kurallarının, EF Core veri erişiminin, API controller'larının ve kullanıcı arayüzünün büyük bölümü `MKFiloServis.Web` projesinde toplanmıştır. Ortak entity ve DTO'lar `MKFiloServis.Shared` projesindedir. PostgreSQL, SQLite, SQL Server ve MySQL çalışma zamanı sağlayıcısı olarak desteklenmektedir; PostgreSQL kanonik migration otoritesi olarak tanımlanmıştır.

Mevcut solution derlenmektedir. Derlemeyi engelleyen hata yoktur. Tek teknik uyarı, `WebDriverManager` üzerinden geçişli gelen `AngleSharp 1.1.2` paketindeki orta önem seviyeli güvenlik açığıdır.

## 2. Solution ve Proje Yapısı

| Proje | Hedef framework | Tür | Sorumluluk |
|---|---|---|---|
| `MKFiloServis.Web` | `net10.0` | ASP.NET Core Web | Blazor UI, EF Core, servisler, API controller'ları ve arka plan işleri |
| `MKFiloServis.Shared` | `net10.0` | Class Library | Entity, DTO, exception ve ortak servis sözleşmeleri |
| `MKFiloServis.DataSync` | `net10.0-windows` | WinForms/CLI | PostgreSQL verisini SQLite'a aktarma |
| `MKFiloServis.LisansDesktop` | `net8.0-windows` | WinForms | Masaüstü lisans yönetimi |
| `MKFiloServis.PlaywrightSmoke` | `net10.0` | Console | Playwright smoke-test çalıştırıcısı |

### Solution dışında kalan dizinler

- `MKFiloServis.Infrastructure` solution'a dahil değildir ve yalnızca boş görünen bir `Data/ApplicationDbContext.cs` dosyası içerir.
- `MKFiloServis.Service` altında `Contracts` ve `Implementations` dizinleri vardır; ancak aktif proje veya kaynak dosyası yoktur.
- Bu dizinler eski veya planlanmış bir katman ayrımının kalıntısı olabilir. Temizlenmeden önce geçmiş ve dış bağımlılıklar doğrulanmalıdır.

### SDK seçimi

Depoda `global.json` bulunmamaktadır. İnceleme sırasında kullanılan SDK:

```text
.NET SDK 10.0.302
MSBuild 18.6.11
```

SDK sürümü sabitlenmediği için farklı geliştirme ve CI makineleri farklı uyumlu SDK patch sürümlerini seçebilir.

## 3. .NET ve EF Core Sürümleri

### Web projesinde bildirilen EF Core paketleri

| Paket | Sürüm |
|---|---:|
| `Microsoft.EntityFrameworkCore.Design` | 10.0.5 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.5 |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.9 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.1 |
| `MySql.EntityFrameworkCore` | 10.0.7 |

NuGet çözümlemesinde temel `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Relational` ve ilgili abstraction paketleri `10.0.9` sürümüne yükseltilmektedir. Build başarılı olsa da sağlayıcıların farklı patch sürümlerinde olması uyumluluk, migration üretimi ve çalışma zamanı davranışı açısından izlenmesi gereken bir risktir.

### Diğer projeler

- DataSync: EF Core SQLite `10.0.9`, Npgsql EF Core `10.0.1`.
- LisansDesktop: `Microsoft.Data.Sqlite 10.0.9`; EF Core kullanılmıyor.

## 4. Veritabanı Mimarisi

Ana uygulama aşağıdaki sağlayıcıları desteklemektedir:

1. PostgreSQL / Npgsql
2. SQLite
3. Microsoft SQL Server
4. MySQL

`Program.cs` içinde sağlayıcı çalışma zamanında seçilerek `AddPooledDbContextFactory<ApplicationDbContext>` yapılandırılmaktadır.

### Ortam tercihleri

- Geliştirme varsayılanı: SQLite
- PreProduction: PostgreSQL
- Production: PostgreSQL
- Kanonik migration otoritesi: PostgreSQL

### DbContext yaklaşımı

- Ana `ApplicationDbContext` yaklaşık 206 `DbSet` içerir.
- Yaklaşık 155 migration bulunmaktadır.
- Varsayılan sorgu davranışı `NoTracking` olarak ayarlanmıştır.
- CRUD işlemlerinin açıkça tracking talep etmesi beklenmektedir.
- Soft-delete global query filter'ları yaygın olarak kullanılmaktadır.
- `IFirmaTenant` ve aktif firma sağlayıcısı üzerinden firma/tenant veri izolasyonu uygulanmaktadır.
- `ScopedDbContextFactory`, Blazor circuit scope içindeki servis sağlayıcısını context'e aktarır.
- Holding entity'leri de ana `ApplicationDbContext` içine alınmıştır; ayrı `HoldingDbContext` nihai mimaride kullanılmamaktadır.

## 5. Katman Analizi

### 5.1 Entity ve model katmanı

`MKFiloServis.Shared/Entities` altında:

- 103 C# kaynak dosyası,
- yaklaşık 245 public sınıf/model bildirimi bulunmaktadır.

Başlıca iş alanları:

- Firma, organizasyon ve şube
- Cari, araç, sürücü ve güzergâh
- Personel, maaş, bordro, izin ve puantaj
- Hakediş ve operasyon kayıtları
- Fatura, banka, finans ve muhasebe
- CRM, WhatsApp ve bildirimler
- EBYS ve belge yönetimi
- Stok, lastik ve bakım
- İhale, satış ve kiralama
- Servis kontratları ve taşıma tedarikçileri

`BaseEntity`, soft-delete ve ortak kayıt alanlarını; `IFirmaTenant` firma izolasyonunu; `IKopyalanabilirTenant` ise tenant verisi kopyalama davranışını desteklemektedir.

### 5.2 Service katmanı

`MKFiloServis.Web/Services` altında:

- 275 C# dosyası,
- yaklaşık 154 service sınıfı,
- yaklaşık 120 service interface'i bulunmaktadır.

Servisler çoğunlukla `Scoped` yaşam süresiyle DI container'a kaydedilmiştir. State veya genel önbellek tutan sınırlı sayıdaki servis `Singleton` olarak çalışmaktadır.

Servis sorumlulukları yalnızca CRUD ile sınırlı değildir. Katman ayrıca:

- Raporlama ve analiz,
- Excel/PDF üretimi,
- Yedekleme ve veri kurtarma,
- E-posta, SMS, Teams, Slack ve WhatsApp entegrasyonu,
- Zamanlanmış Quartz işleri,
- Yapay zekâ/ML özellikleri,
- Dosya ve belge güvenliği,
- Veri aktarımı ve migration yardımcıları

gibi altyapı görevlerini de barındırmaktadır. Bu nedenle Web projesinin sorumluluk alanı oldukça geniştir.

### 5.3 Controller katmanı

Toplam 14 controller bulunmaktadır:

- `AnalitikController`
- `AraclarController`
- `AracTakipController`
- `AuthController`
- `CarilerController`
- `DosyaController`
- `FaturaGrupSablonuController`
- `FaturalarController`
- `GuzergahlarController`
- `HealthController`
- `LicenseController`
- `PuantajIstisnaController`
- `SoforlerController`
- `SystemHealthController`

Controller endpoint'leri `MapControllers()` ile Blazor endpoint'lerinin yanında yayınlanmaktadır. Katman işlevsel olarak API, kimlik doğrulama, sağlık kontrolü ve dosya erişimi sunmaktadır.

### 5.4 Razor/Blazor katmanı

- 282 Razor bileşeni,
- 280 `@page` route bildirimi bulunmaktadır.

Uygulama Interactive Server render mode kullanır. Route ağacı `AuthorizeRouteView` ile kimlik doğrulama/yetkilendirme kontrolünden geçirilir.

En yoğun UI alanları:

| Alan | Yaklaşık Razor dosyası |
|---|---:|
| Raporlar | 35 |
| Ayarlar | 34 |
| Personel | 17 |
| Muhasebe | 16 |
| Admin | 9 |
| Destek Talepleri | 8 |
| EBYS | 7 |
| Holding | 7 |

Ana sayfa düzeni `Components/Pages` altındadır. Bunun yanında `Web/Pages/Puantaj` altında iki paralel Razor dosyası bulunması, sayfa konvansiyonunun tamamen tekilleşmediğini göstermektedir.

## 6. Build ve Mevcut Hatalar

Çalıştırılan komut:

```powershell
dotnet build MKFiloServis.slnx --nologo --verbosity:minimal
```

Sonuç:

```text
Build başarılı
0 hata
2 uyarı
```

İki uyarı aynı NuGet güvenlik bulgusunun restore ve build aşamalarında tekrarlanmasıdır:

```text
NU1902: AngleSharp 1.1.2 paketinde bilinen orta önem seviyeli güvenlik açığı
```

Bağımlılık zinciri:

```text
MKFiloServis.Web
└── WebDriverManager 2.17.7
    └── AngleSharp 1.1.2
```

`AngleSharp` doğrudan proje referansı değildir. Çözüm, öncelikle `WebDriverManager` paketinin güvenli ve uyumlu bir sürüme yükseltilip yükseltilemeyeceğini doğrulamalıdır.

Build bütün projeleri başarıyla üretmiştir. Playwright smoke-test projesi yalnızca derlenmiş, tarayıcı testi çalıştırılmamıştır. Solution'da standart bir unit/integration test projesi tespit edilmemiştir.

## 7. Riskler ve Teknik Borçlar

### Yüksek öncelik

1. Çalışma ağacında analizden önce var olan çok sayıda değiştirilmiş ve yeni dosya vardır. Yeni çalışma mevcut değişikliklerden izole yürütülmelidir.
2. `AngleSharp 1.1.2` için bilinen orta önem seviyeli güvenlik açığı vardır.
3. EF Core sağlayıcı paketleri farklı patch sürümlerindedir.

### Orta öncelik

1. `global.json` olmadığı için SDK seçimi makineler arasında değişebilir.
2. Web projesi veri erişimi, iş mantığı, entegrasyonlar, arka plan işleri ve UI'ı birlikte taşımaktadır.
3. 206 `DbSet` içeren tek context'in model oluşturma, migration ve bakım maliyeti yüksektir.
4. Dört veritabanı sağlayıcısı desteklenmesine rağmen migration otoritesinin tek sağlayıcı olması, sağlayıcıya özgü davranışların ayrıca test edilmesini gerektirir.
5. Aktif olmayan `Infrastructure` ve `Service` dizinleri mimariyi okuyan geliştiriciler için yanıltıcı olabilir.
6. Otomatik test kapsamı görünür biçimde sınırlıdır.

## 8. Uygulanabilir Çalışma Planı

Plan uygulanmadan önce açık onay alınmalıdır.

### Aşama 1 — Baseline ve değişiklik izolasyonu

1. Mevcut değiştirilmiş/yeni dosyaların listesini baseline olarak kaydet.
2. Kullanıcının mevcut çalışmalarını değiştirme veya yeniden biçimlendirme.
3. Yapılacak iş için etkilenecek dosyaları önceden sınırla.

### Aşama 2 — Değişiklik tasarımı

1. İstenen değişikliği entity → DbContext/migration → service → controller → Razor akışı boyunca modelle.
2. Tenant filtresi, soft-delete, authorization ve context yaşam süresi etkilerini belirle.
3. PostgreSQL, SQLite, SQL Server ve MySQL uyumluluğunu tasarım aşamasında kontrol et.

### Aşama 3 — Kontrollü uygulama

1. Ortak entity/DTO/sözleşmeleri uygun olduğunda `Shared` projesinde tanımla.
2. Servis interface ve implementasyonunu Web servis katmanında oluştur.
3. Gerekliyse controller/API sözleşmesini ekle veya güncelle.
4. Razor bileşenlerini mevcut `Components/Pages` ve Interactive Server konvansiyonuna uygun geliştir.
5. Sağlayıcıya özel SQL gerekiyorsa açık sağlayıcı dalları ve karşılık gelen testleri ekle.

### Aşama 4 — Migration ve veri doğrulama

1. Migration'ı PostgreSQL kanonik modeline göre üret.
2. Model snapshot diff'ini manuel incele.
3. SQLite geliştirme veritabanında migration davranışını doğrula.
4. Etkilenen özellik diğer sağlayıcılarda kullanılacaksa SQL Server ve MySQL kontrollerini de çalıştır.
5. Geri dönüş ve veri kaybı risklerini raporla.

### Aşama 5 — Test ve kalite kapıları

1. Değiştirilen iş kuralları için hedefli otomatik test ekle.
2. İlgili Razor/API akışı için smoke senaryosu çalıştır.
3. Solution build'ini yeniden çalıştır.
4. NuGet güvenlik uyarılarını kontrol et.
5. Yalnızca amaçlanan dosyaların değiştiğini `git diff` ile doğrula.

### Aşama 6 — Ayrı bakım işleri

Fonksiyonel değişiklikten ayrı commit/değişiklik grubu olarak:

1. `WebDriverManager` ve geçişli `AngleSharp` güvenlik sorununu gider.
2. EF Core sağlayıcı patch sürümlerini uyumlu sürüm matrisine göre düzenle.
3. Uygun .NET SDK sürümünü `global.json` ile sabitlemeyi değerlendir.
4. Kullanılmayan `Infrastructure` ve `Service` dizinlerinin geçmişini inceleyip arşivleme/temizleme kararı al.
5. Temel unit ve integration test projeleri oluşturmayı değerlendir.

## 9. Onay Noktası

Kaynak kod değişikliğine geçmeden önce aşağıdaki kapsamın onaylanması önerilir:

- Mevcut kullanıcı değişiklikleri korunacak.
- Yeni iş, katman ve dosya bazında sınırlandırılacak.
- Çoklu veritabanı uyumluluğu zorunlu kabul edilecek.
- Migration değişiklikleri ayrıca incelenecek.
- Güvenlik/paket güncellemeleri fonksiyonel geliştirmeden ayrı ele alınacak.

Bu rapor analiz ve planlama belgesidir; fonksiyonel kaynak kod değişikliği içermez.

## 10. Onay Sonrası Seçilen İlk Bakım İşinin Sonucu

**Uygulama tarihi:** 2 Ağustos 2026

İlk çalışma olarak baseline build'deki `WebDriverManager → AngleSharp 1.1.2` güvenlik uyarısının giderilmesi seçilmiştir.

### Uygulanan değişiklik

- `MKFiloServis.Web.csproj` dosyasına doğrudan `AngleSharp 1.5.0` paket referansı eklenmiştir.
- `WebDriverManager 2.17.7` korunmuştur; Selenium scraper içindeki aktif kullanım davranışı değiştirilmemiştir.
- GitHub güvenlik kaydına göre `< 1.5.0` sürümleri etkilenmektedir ve ilk düzeltilmiş sürüm `1.5.0`dır.

Kaynaklar:

- https://github.com/advisories/GHSA-pgww-w46g-26qg
- https://www.nuget.org/packages/WebDriverManager/
- https://www.selenium.dev/selenium/docs/api/dotnet/webdriver/OpenQA.Selenium.DriverFinder.html

### Doğrulama

Normal build, çalışan `MKFiloServis.Web.exe` sürecinin varsayılan çıktı dosyasını kilitlemesi nedeniyle `MSB3027/MSB3021` ile tamamlanamamıştır. Çalışan kullanıcı süreci sonlandırılmamış; build ayrı artifacts yolunda yeniden çalıştırılmıştır:

```powershell
dotnet build MKFiloServis.slnx --nologo --verbosity:minimal --artifacts-path artifacts/security-validation
```

Sonuç:

```text
Build başarılı
0 hata
3 derleyici uyarısı
AngleSharp çözümlemesi: 1.5.0
Önceki AngleSharp NU1902 uyarısı: giderildi
```

Kalan üç uyarı seçilen paket değişikliğinden bağımsızdır:

- `MKFiloServis.LisansDesktop/MainForm.cs`: kullanılmayan `_suppressHistoryRefresh` alanı (`CS0169`).
- `MKFiloServis.LisansDesktop/MainForm.cs`: hiç atanmayan `_isLoadingSelection` alanı (`CS0649`).
- `MKFiloServis.Web/Components/Pages/Guzergahlar/GuzergahForm.razor`: değeri kullanılmayan `seferTipiDegistiUyarisi` alanı (`CS0414`).

### Kalan güvenlik bulgusu

`dotnet list package --vulnerable --include-transitive` denetimi, `SQLitePCLRaw.lib.e_sqlite3 2.1.11` için yüksek önem seviyeli `GHSA-2m69-gcr7-jv3q` bulgusunu raporlamaktadır. Bu bildirim proje dosyalarında önceden `NuGetAuditSuppress` ile bastırılmıştır. Bastırmanın gerekçesi ve güncel güvenli yükseltme yolu ayrı bir bakım işi olarak incelenmelidir; bu değişiklik kapsamında suppression kaldırılmamış veya SQLite bağımlılık zinciri değiştirilmemiştir.
