using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KOAFiloServis.Web.Services;

/// <summary>
/// Multi-tenant (çoklu şirket) yönetimi servis implementasyonu
/// </summary>
public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _context;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantService> _logger;

    private int? _currentSirketId;
    private Sirket? _currentSirket;
    private bool _isInitialized;

    public TenantService(
        ApplicationDbContext context,
        IDbContextFactory<ApplicationDbContext> contextFactory,
        AuthenticationStateProvider authStateProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TenantService> logger)
    {
        _context = context;
        _contextFactory = contextFactory;
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public int? CurrentSirketId
    {
        get
        {
            EnsureInitialized();
            return _currentSirketId;
        }
    }

    public Sirket? CurrentSirket
    {
        get
        {
            EnsureInitialized();
            return _currentSirket;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var user = GetCurrentUser();
            if (user == null) return false;

            // SuperAdmin claim veya Admin/SuperAdmin rolü
            var isSuperAdminClaim = user.FindFirst("IsSuperAdmin")?.Value == "true";
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            return isSuperAdminClaim || role == "SuperAdmin" || role == "Admin";
        }
    }

    public bool HasAccessToSirket(int sirketId)
    {
        if (IsSuperAdmin)
            return true;

        return CurrentSirketId == sirketId;
    }

    public async Task SetCurrentSirketAsync(int? sirketId)
    {
        if (!IsSuperAdmin && sirketId != CurrentSirketId)
        {
            throw new UnauthorizedAccessException("Şirket değiştirme yetkiniz yok.");
        }

        if (sirketId.HasValue)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            _currentSirket = await context.Sirketler
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == sirketId.Value && !s.IsDeleted && s.Aktif);

            if (_currentSirket == null)
            {
                throw new InvalidOperationException("Geçersiz şirket ID.");
            }

            _currentSirketId = sirketId.Value;
        }
        else
        {
            _currentSirketId = null;
            _currentSirket = null;
        }

        // Session/Cookie'ye kaydet (Blazor Server'da session mevcut olmayabilir)
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>() != null)
            {
                httpContext.Session.SetInt32("CurrentSirketId", sirketId ?? 0);
            }
        }
        catch (InvalidOperationException)
        {
            // Session yapılandırılmamış - Blazor Server'da normal durum
        }

        _logger.LogInformation("Tenant context değişti: SirketId={SirketId}", sirketId);
    }

    public async Task<List<Sirket>> GetAllSirketlerAsync()
    {
        if (!IsSuperAdmin)
        {
            // Normal kullanıcı sadece kendi şirketini görebilir
            if (CurrentSirketId.HasValue)
            {
                var sirket = await GetSirketByIdAsync(CurrentSirketId.Value);
                return sirket != null ? [sirket] : [];
            }
            return [];
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Sirketler
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.SirketKodu)
            .ToListAsync();
    }

    public async Task<Sirket?> GetSirketByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Sirketler
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<Sirket> CreateSirketAsync(SirketOlusturModel model)
    {
        if (!IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Şirket oluşturma yetkiniz yok.");
        }

        // Şirket kodu benzersizlik kontrolü
        if (!await IsSirketKoduUniqueAsync(model.SirketKodu))
        {
            throw new InvalidOperationException($"'{model.SirketKodu}' şirket kodu zaten kullanılıyor.");
        }

        var sirket = new Sirket
        {
            SirketKodu = model.SirketKodu.ToUpperInvariant(),
            Unvan = model.Unvan,
            KisaAd = model.KisaAd,
            VergiDairesi = model.VergiDairesi,
            VergiNo = model.VergiNo,
            Adres = model.Adres,
            Il = model.Il,
            Ilce = model.Ilce,
            Telefon = model.Telefon,
            Email = model.Email,
            WebSitesi = model.WebSitesi,
            ParaBirimi = model.ParaBirimi,
            MaxKullaniciSayisi = model.MaxKullaniciSayisi,
            Aktif = true
        };

        await using var context = await _contextFactory.CreateDbContextAsync();
        context.Sirketler.Add(sirket);
        await context.SaveChangesAsync();

        _logger.LogInformation("Yeni şirket oluşturuldu: {SirketKodu} - {Unvan}", sirket.SirketKodu, sirket.Unvan);
        return sirket;
    }

    public async Task<Sirket> UpdateSirketAsync(SirketGuncelleModel model)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var sirket = await context.Sirketler.FindAsync(model.Id)
            ?? throw new InvalidOperationException("Şirket bulunamadı.");

        if (!HasAccessToSirket(model.Id))
        {
            throw new UnauthorizedAccessException("Bu şirketi güncelleme yetkiniz yok.");
        }

        sirket.Unvan = model.Unvan;
        sirket.KisaAd = model.KisaAd;
        sirket.VergiDairesi = model.VergiDairesi;
        sirket.VergiNo = model.VergiNo;
        sirket.Adres = model.Adres;
        sirket.Il = model.Il;
        sirket.Ilce = model.Ilce;
        sirket.Telefon = model.Telefon;
        sirket.Email = model.Email;
        sirket.WebSitesi = model.WebSitesi;
        sirket.LogoUrl = model.LogoUrl;
        sirket.ParaBirimi = model.ParaBirimi;
        sirket.Aktif = model.Aktif;
        sirket.MaxKullaniciSayisi = model.MaxKullaniciSayisi;
        sirket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation("Şirket güncellendi: {SirketKodu}", sirket.SirketKodu);
        return sirket;
    }

    public async Task DeleteSirketAsync(int id)
    {
        if (!IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Şirket silme yetkiniz yok.");
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        var sirket = await context.Sirketler.FindAsync(id)
            ?? throw new InvalidOperationException("Şirket bulunamadı.");

        // Şirkete bağlı aktif kullanıcı var mı kontrol et
        var aktifKullaniciSayisi = await context.Kullanicilar
            .CountAsync(k => k.SirketId == id && !k.IsDeleted && k.Aktif);

        if (aktifKullaniciSayisi > 0)
        {
            throw new InvalidOperationException(
                $"Bu şirkete bağlı {aktifKullaniciSayisi} aktif kullanıcı var. Önce kullanıcıları pasif yapın veya silin.");
        }

        sirket.IsDeleted = true;
        sirket.Aktif = false;
        sirket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        _logger.LogInformation("Şirket silindi: {SirketKodu}", sirket.SirketKodu);
    }

    public async Task<bool> IsSirketKoduUniqueAsync(string sirketKodu, int? excludeId = null)
    {
        var normalizedKod = sirketKodu.ToUpperInvariant();

        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.Sirketler
            .Where(s => s.SirketKodu == normalizedKod && !s.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    private void EnsureInitialized()
    {
        if (_isInitialized)
            return;

        var user = GetCurrentUser();
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Claim'den SirketId al
            var sirketIdClaim = user.FindFirst("SirketId")?.Value;
            if (int.TryParse(sirketIdClaim, out var sirketId))
            {
                _currentSirketId = sirketId;
                // Lazy load şirket bilgisi
                    using var context = _contextFactory.CreateDbContext();
                    _currentSirket = context.Sirketler
                    .AsNoTracking()
                    .FirstOrDefault(s => s.Id == sirketId && !s.IsDeleted);
            }

            // Session'dan override kontrolü (super admin için)
            // Not: Blazor Server'da session her zaman mevcut olmayabilir
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>() != null && IsSuperAdmin)
                {
                    var sessionSirketId = httpContext.Session.GetInt32("CurrentSirketId");
                    if (sessionSirketId.HasValue && sessionSirketId.Value > 0)
                    {
                        _currentSirketId = sessionSirketId.Value;
                        using var context = _contextFactory.CreateDbContext();
                        _currentSirket = context.Sirketler
                            .AsNoTracking()
                            .FirstOrDefault(s => s.Id == sessionSirketId.Value && !s.IsDeleted);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Session yapılandırılmamış - Blazor Server'da normal durum
            }
        }

        _isInitialized = true;
    }

    private ClaimsPrincipal? GetCurrentUser()
    {
        // HTTP Context'ten al
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User;
        }

        // Blazor AuthenticationStateProvider'dan al
        try
        {
            var authState = _authStateProvider.GetAuthenticationStateAsync().Result;
            return authState.User;
        }
        catch
        {
            return null;
        }
    }

    #region Şirketler Arası Transfer

    public async Task<SirketTransferResult> TransferAsync(SirketTransferRequest request)
    {
        if (!IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Şirketler arası transfer için SuperAdmin yetkisi gerekli.");
        }

        var result = new SirketTransferResult();

        // Hedef şirket kontrolü
        await using var context = await _contextFactory.CreateDbContextAsync();

        var hedefSirket = await context.Sirketler
            .FirstOrDefaultAsync(s => s.Id == request.HedefSirketId && !s.IsDeleted && s.Aktif);

        if (hedefSirket == null)
        {
            result.Hatalar.Add("Hedef şirket bulunamadı veya aktif değil.");
            return result;
        }

        var kullaniciId = GetCurrentUserId();

        foreach (var entityId in request.EntityIdler)
        {
            try
            {
                var transferLog = await TransferEntityAsync(
                    request.EntityTuru, 
                    entityId, 
                    request.HedefSirketId,
                    request.IliskiliVerileriTransferEt,
                    kullaniciId,
                    request.Notlar);

                if (transferLog.Durum == TransferDurum.Basarili)
                {
                    result.TransferEdilenSayisi++;
                    result.IliskiliEntitySayisi += transferLog.IliskiliEntitySayisi;
                }
                else
                {
                    result.BasarisizSayisi++;
                    if (!string.IsNullOrEmpty(transferLog.HataMesaji))
                    {
                        result.Hatalar.Add(transferLog.HataMesaji);
                    }
                }

                result.TransferLogIdler.Add(transferLog.Id);
            }
            catch (Exception ex)
            {
                result.BasarisizSayisi++;
                result.Hatalar.Add($"Entity ID {entityId}: {ex.Message}");
                _logger.LogError(ex, "Transfer hatası: EntityTuru={EntityTuru}, EntityId={EntityId}", 
                    request.EntityTuru, entityId);
            }
        }

        result.Basarili = result.BasarisizSayisi == 0;
        return result;
    }

    private async Task<SirketTransferLog> TransferEntityAsync(
        string entityTuru, 
        int entityId, 
        int hedefSirketId,
        bool iliskiliVerileriTransferEt,
        int kullaniciId,
        string? notlar)
    {
        var log = new SirketTransferLog
        {
            EntityTuru = entityTuru,
            EntityId = entityId,
            HedefSirketId = hedefSirketId,
            KullaniciId = kullaniciId,
            TransferTarihi = DateTime.UtcNow,
            IliskiliVerilerTransferEdildi = iliskiliVerileriTransferEt,
            Notlar = notlar
        };

        try
        {
            switch (entityTuru)
            {
                case TransferEntityTurleri.Cari:
                    await TransferCariAsync(entityId, hedefSirketId, iliskiliVerileriTransferEt, log);
                    break;

                case TransferEntityTurleri.Arac:
                    await TransferAracAsync(entityId, hedefSirketId, iliskiliVerileriTransferEt, log);
                    break;

                case TransferEntityTurleri.Sofor:
                    await TransferSoforAsync(entityId, hedefSirketId, iliskiliVerileriTransferEt, log);
                    break;

                case TransferEntityTurleri.Guzergah:
                    await TransferGuzergahAsync(entityId, hedefSirketId, log);
                    break;

                case TransferEntityTurleri.Fatura:
                    await TransferFaturaAsync(entityId, hedefSirketId, log);
                    break;

                case TransferEntityTurleri.BankaHesap:
                    await TransferBankaHesapAsync(entityId, hedefSirketId, iliskiliVerileriTransferEt, log);
                    break;

                case TransferEntityTurleri.BankaKasaHareket:
                    await TransferBankaKasaHareketAsync(entityId, hedefSirketId, log);
                    break;

                default:
                    throw new InvalidOperationException($"Desteklenmeyen entity türü: {entityTuru}");
            }

            log.Durum = TransferDurum.Basarili;
        }
        catch (Exception ex)
        {
            log.Durum = TransferDurum.Basarisiz;
            log.HataMesaji = ex.Message;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        context.SirketTransferLoglari.Add(log);
        await context.SaveChangesAsync();

        return log;
    }

    private async Task TransferCariAsync(int cariId, int hedefSirketId, bool iliskiliVeriler, SirketTransferLog log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var cari = await context.Cariler.FindAsync(cariId)
            ?? throw new InvalidOperationException("Cari bulunamadı.");

        log.KaynakSirketId = 0; // Legacy SirketId Cari'den kaldırıldı (Teknik Borç #5).
        log.EntityAciklama = $"{cari.CariKodu} - {cari.Unvan}";

        // Legacy Cari.SirketId kolonu drop edildi; tenant izolasyonu artık FirmaId ile sağlanıyor.
        cari.UpdatedAt = DateTime.UtcNow;

        if (iliskiliVeriler)
        {
            // İlişkili faturaları da transfer et
            var faturalar = await context.Faturalar
                .Where(f => f.CariId == cariId && !f.IsDeleted)
                .ToListAsync();

            foreach (var fatura in faturalar)
            {
                // Legacy Fatura.SirketId kolonu drop edildi (Teknik Borç #5).
                fatura.UpdatedAt = DateTime.UtcNow;
            }

            log.IliskiliEntitySayisi = faturalar.Count;
        }

        await context.SaveChangesAsync();
    }

    private async Task TransferAracAsync(int aracId, int hedefSirketId, bool iliskiliVeriler, SirketTransferLog log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var arac = await context.Araclar.FindAsync(aracId)
            ?? throw new InvalidOperationException("Araç bulunamadı.");

        log.KaynakSirketId = arac.SirketId ?? 0;
        log.EntityAciklama = $"{arac.Plaka} - {arac.Marka} {arac.Model}";

        arac.SirketId = hedefSirketId;
        arac.UpdatedAt = DateTime.UtcNow;

        if (iliskiliVeriler)
        {
            // İlişkili masrafları da transfer et
            var masraflar = await context.AracMasraflari
                .Where(m => m.AracId == aracId && !m.IsDeleted)
                .ToListAsync();

            // Araç evraklarını da transfer et (SirketId yok, araç üzerinden takip ediliyor)
            log.IliskiliEntitySayisi = masraflar.Count;
        }

        await context.SaveChangesAsync();
    }

    private async Task TransferSoforAsync(int soforId, int hedefSirketId, bool iliskiliVeriler, SirketTransferLog log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var sofor = await context.Soforler.FindAsync(soforId)
            ?? throw new InvalidOperationException("Şoför bulunamadı.");

        log.KaynakSirketId = sofor.SirketId ?? 0;
        log.EntityAciklama = $"{sofor.SoforKodu} - {sofor.Ad} {sofor.Soyad}";

        sofor.SirketId = hedefSirketId;
        sofor.UpdatedAt = DateTime.UtcNow;

        if (iliskiliVeriler)
        {
            // İlişkili puantajları da transfer et
            var puantajlar = await context.PersonelPuantajlar
                .Where(p => p.PersonelId == soforId && !p.IsDeleted)
                .ToListAsync();

            log.IliskiliEntitySayisi = puantajlar.Count;
        }

        await context.SaveChangesAsync();
    }

    private async Task TransferGuzergahAsync(int guzergahId, int hedefSirketId, SirketTransferLog log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var guzergah = await context.Guzergahlar.FindAsync(guzergahId)
            ?? throw new InvalidOperationException("Güzergah bulunamadı.");

        log.KaynakSirketId = guzergah.SirketId ?? 0;
        log.EntityAciklama = $"{guzergah.GuzergahKodu} - {guzergah.GuzergahAdi}";

        guzergah.SirketId = hedefSirketId;
        guzergah.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    private async Task TransferFaturaAsync(int faturaId, int hedefSirketId, SirketTransferLog log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var fatura = await context.Faturalar
            .Include(f => f.Cari)
            .FirstOrDefaultAsync(f => f.Id == faturaId)
            ?? throw new InvalidOperationException("Fatura bulunamadı.");

        log.KaynakSirketId = 0; // Legacy Fatura.SirketId kaldırıldı (Teknik Borç #5).
        log.EntityAciklama = $"{fatura.FaturaNo} - {fatura.Cari?.Unvan}";

        // Legacy Fatura.SirketId kolonu drop edildi; tenant izolasyonu artık FirmaId ile sağlanıyor.
        fatura.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    private async Task TransferBankaHesapAsync(int hesapId, int hedefSirketId, bool iliskiliVeriler, SirketTransferLog log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var hesap = await context.BankaHesaplari.FindAsync(hesapId)
            ?? throw new InvalidOperationException("Banka hesabı bulunamadı.");

        log.KaynakSirketId = hesap.SirketId ?? 0;
        log.EntityAciklama = $"{hesap.HesapKodu} - {hesap.HesapAdi}";

        hesap.SirketId = hedefSirketId;
        hesap.UpdatedAt = DateTime.UtcNow;

        if (iliskiliVeriler)
        {
            // İlişkili hareketleri de transfer et
            var hareketler = await context.BankaKasaHareketleri
                .Where(h => h.BankaHesapId == hesapId && !h.IsDeleted)
                .ToListAsync();

            foreach (var hareket in hareketler)
            {
                hareket.SirketId = hedefSirketId;
                hareket.UpdatedAt = DateTime.UtcNow;
            }

            log.IliskiliEntitySayisi = hareketler.Count;
        }

        await context.SaveChangesAsync();
    }

    private async Task TransferBankaKasaHareketAsync(int hareketId, int hedefSirketId, SirketTransferLog log)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var hareket = await context.BankaKasaHareketleri
            .Include(h => h.BankaHesap)
            .FirstOrDefaultAsync(h => h.Id == hareketId)
            ?? throw new InvalidOperationException("Banka/Kasa hareketi bulunamadı.");

        log.KaynakSirketId = hareket.SirketId ?? 0;
        log.EntityAciklama = $"{hareket.BankaHesap?.HesapAdi} - {hareket.Tutar:N2} TL";

        hareket.SirketId = hedefSirketId;
        hareket.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task<List<TransferEntityOzet>> GetTransferOnizlemeAsync(string entityTuru, List<int> entityIdler)
    {
        if (!IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Transfer önizleme için SuperAdmin yetkisi gerekli.");
        }

        var ozetler = new List<TransferEntityOzet>();

        foreach (var entityId in entityIdler)
        {
            var ozet = entityTuru switch
            {
                TransferEntityTurleri.Cari => await GetCariOzetAsync(entityId),
                TransferEntityTurleri.Arac => await GetAracOzetAsync(entityId),
                TransferEntityTurleri.Sofor => await GetSoforOzetAsync(entityId),
                TransferEntityTurleri.Guzergah => await GetGuzergahOzetAsync(entityId),
                TransferEntityTurleri.Fatura => await GetFaturaOzetAsync(entityId),
                TransferEntityTurleri.BankaHesap => await GetBankaHesapOzetAsync(entityId),
                TransferEntityTurleri.BankaKasaHareket => await GetBankaKasaHareketOzetAsync(entityId),
                _ => null
            };

            if (ozet != null)
            {
                ozetler.Add(ozet);
            }
        }

        return ozetler;
    }

    private async Task<TransferEntityOzet?> GetCariOzetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var cari = await context.Cariler
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (cari == null) return null;

        var faturaCount = await context.Faturalar
            .CountAsync(f => f.CariId == id && !f.IsDeleted);

        return new TransferEntityOzet
        {
            EntityTuru = TransferEntityTurleri.Cari,
            Id = cari.Id,
            Aciklama = $"{cari.CariKodu} - {cari.Unvan}",
            MevcutSirketId = null,
            MevcutSirketAdi = null,
            IliskiliEntitySayisi = faturaCount
        };
    }

    private async Task<TransferEntityOzet?> GetAracOzetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var arac = await context.Araclar
            .AsNoTracking()
            .Include(a => a.Sirket)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (arac == null) return null;

        var masrafCount = await context.AracMasraflari
            .CountAsync(m => m.AracId == id && !m.IsDeleted);

        return new TransferEntityOzet
        {
            EntityTuru = TransferEntityTurleri.Arac,
            Id = arac.Id,
            Aciklama = $"{arac.Plaka} - {arac.Marka} {arac.Model}",
            MevcutSirketId = arac.SirketId,
            MevcutSirketAdi = arac.Sirket?.KisaAd ?? arac.Sirket?.Unvan,
            IliskiliEntitySayisi = masrafCount
        };
    }

    private async Task<TransferEntityOzet?> GetSoforOzetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var sofor = await context.Soforler
            .AsNoTracking()
            .Include(s => s.Sirket)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (sofor == null) return null;

        var puantajCount = await context.PersonelPuantajlar
            .CountAsync(p => p.PersonelId == id && !p.IsDeleted);

        return new TransferEntityOzet
        {
            EntityTuru = TransferEntityTurleri.Sofor,
            Id = sofor.Id,
            Aciklama = $"{sofor.SoforKodu} - {sofor.Ad} {sofor.Soyad}",
            MevcutSirketId = sofor.SirketId,
            MevcutSirketAdi = sofor.Sirket?.KisaAd ?? sofor.Sirket?.Unvan,
            IliskiliEntitySayisi = puantajCount
        };
    }

    private async Task<TransferEntityOzet?> GetGuzergahOzetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var guzergah = await context.Guzergahlar
            .AsNoTracking()
            .Include(g => g.Sirket)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

        if (guzergah == null) return null;

        return new TransferEntityOzet
        {
            EntityTuru = TransferEntityTurleri.Guzergah,
            Id = guzergah.Id,
            Aciklama = $"{guzergah.GuzergahKodu} - {guzergah.GuzergahAdi}",
            MevcutSirketId = guzergah.SirketId,
            MevcutSirketAdi = guzergah.Sirket?.KisaAd ?? guzergah.Sirket?.Unvan,
            IliskiliEntitySayisi = 0
        };
    }

    private async Task<TransferEntityOzet?> GetFaturaOzetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var fatura = await context.Faturalar
            .AsNoTracking()
            .Include(f => f.Cari)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);

        if (fatura == null) return null;

        return new TransferEntityOzet
        {
            EntityTuru = TransferEntityTurleri.Fatura,
            Id = fatura.Id,
            Aciklama = $"{fatura.FaturaNo} - {fatura.Cari?.Unvan}",
            MevcutSirketId = null,
            MevcutSirketAdi = null,
            IliskiliEntitySayisi = 0
        };
    }

    private async Task<TransferEntityOzet?> GetBankaHesapOzetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var hesap = await context.BankaHesaplari
            .AsNoTracking()
            .Include(h => h.Sirket)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

        if (hesap == null) return null;

        var hareketCount = await context.BankaKasaHareketleri
            .CountAsync(h => h.BankaHesapId == id && !h.IsDeleted);

        return new TransferEntityOzet
        {
            EntityTuru = TransferEntityTurleri.BankaHesap,
            Id = hesap.Id,
            Aciklama = $"{hesap.HesapKodu} - {hesap.HesapAdi}",
            MevcutSirketId = hesap.SirketId,
            MevcutSirketAdi = hesap.Sirket?.KisaAd ?? hesap.Sirket?.Unvan,
            IliskiliEntitySayisi = hareketCount
        };
    }

    private async Task<TransferEntityOzet?> GetBankaKasaHareketOzetAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var hareket = await context.BankaKasaHareketleri
            .AsNoTracking()
            .Include(h => h.Sirket)
            .Include(h => h.BankaHesap)
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

        if (hareket == null) return null;

        return new TransferEntityOzet
        {
            EntityTuru = TransferEntityTurleri.BankaKasaHareket,
            Id = hareket.Id,
            Aciklama = $"{hareket.BankaHesap?.HesapAdi} - {hareket.Tutar:N2} TL",
            MevcutSirketId = hareket.SirketId,
            MevcutSirketAdi = hareket.Sirket?.KisaAd ?? hareket.Sirket?.Unvan,
            IliskiliEntitySayisi = 0
        };
    }

    public async Task<List<SirketTransferLog>> GetTransferLoglariAsync(int? sirketId = null, DateTime? baslangic = null, DateTime? bitis = null)
    {
        if (!IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Transfer logları için SuperAdmin yetkisi gerekli.");
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.SirketTransferLoglari
            .AsNoTracking()
            .Include(l => l.KaynakSirket)
            .Include(l => l.HedefSirket)
            .Include(l => l.Kullanici)
            .Where(l => !l.IsDeleted);

        if (sirketId.HasValue)
        {
            query = query.Where(l => l.KaynakSirketId == sirketId || l.HedefSirketId == sirketId);
        }

        if (baslangic.HasValue)
        {
            query = query.Where(l => l.TransferTarihi >= baslangic.Value);
        }

        if (bitis.HasValue)
        {
            query = query.Where(l => l.TransferTarihi <= bitis.Value);
        }

        return await query
            .OrderByDescending(l => l.TransferTarihi)
            .Take(500)
            .ToListAsync();
    }

    public async Task<SirketTransferLog?> GetTransferLogByIdAsync(int id)
    {
        if (!IsSuperAdmin)
        {
            throw new UnauthorizedAccessException("Transfer log detayı için SuperAdmin yetkisi gerekli.");
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.SirketTransferLoglari
            .AsNoTracking()
            .Include(l => l.KaynakSirket)
            .Include(l => l.HedefSirket)
            .Include(l => l.Kullanici)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
    }

    private int GetCurrentUserId()
    {
        var user = GetCurrentUser();
        var userIdClaim = user?.FindFirst("UserId")?.Value ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #endregion
}
