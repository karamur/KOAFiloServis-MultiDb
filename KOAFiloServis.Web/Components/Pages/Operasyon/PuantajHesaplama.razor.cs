using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Services;
using KOAFiloServis.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace KOAFiloServis.Web.Components.Pages.Operasyon;

public partial class PuantajHesaplama : ComponentBase
{
    [Inject] private IPreviewEngineService PreviewEngine { get; set; } = null!;
    [Inject] private IPuantajEngineService Engine { get; set; } = null!;
    [Inject] private IKurumService KurumService { get; set; } = null!;
    [Inject] private IPuantajWorkflowService Workflow { get; set; } = null!;
    [Inject] private IPuantajFinansService FinansService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    // ── Filtre ───────────────────────────────────────────────────────────
    private int seciliYil = DateTime.Today.Year;
    private int seciliAy = DateTime.Today.Month;
    private int? seciliKurumId;
    private string kurumArama = "";
    private List<Kurum> kurumOnerileri = new();
    private List<Kurum> tumKurumlar = new();

    // ── Preview State ────────────────────────────────────────────────────
    private PreviewResult? preview;
    private bool previewYukleniyor;
    private bool hesaplamaYapiliyor;
    private string? hataMesaji;
    private string? sonucMesaji;

    // ── Sonuç ────────────────────────────────────────────────────────────
    private PuantajEngineSonucV1? hesaplamaSonucu;

    // ── Comparison ──────────────────────────────────────────────────────
    private ComparisonResult? comparison;
    private bool comparisonYukleniyor;

    // ── Onay State ──────────────────────────────────────────────────────
    private bool onayYukleniyor;
    private List<PuantajAuditLog> auditLogs = new();

    // ── Finansal State ──────────────────────────────────────────────────
    private bool finansYukleniyor;
    private List<PuantajFinansalKayit> finansalKayitlar = new();
    private int? finansalUretilenFaturaAdet;

    // ── Drill-Down ──────────────────────────────────────────────────────
    private bool drillDownAcik;
    private List<DrillDownOperasyon> drillDownOps = new();
    private string drillDownBaslik = "";
    private bool drillDownYukleniyor;

    protected override async Task OnInitializedAsync()
    {
        tumKurumlar = await KurumService.GetAktifAsync();
    }

    // ── Filtre ──────────────────────────────────────────────────────────

    private void KurumAramaGuncelle(ChangeEventArgs e)
    {
        kurumArama = e?.Value?.ToString() ?? "";
        kurumOnerileri = string.IsNullOrWhiteSpace(kurumArama)
            ? new()
            : tumKurumlar
                .Where(k => (k.KurumAdi ?? "").Contains(kurumArama, StringComparison.OrdinalIgnoreCase))
                .Take(10).ToList();
    }

    private void KurumSec(Kurum k)
    {
        seciliKurumId = k.Id;
        kurumArama = k.KurumAdi ?? "";
        kurumOnerileri = new();
    }

    private void KurumTemizle()
    {
        seciliKurumId = null;
        kurumArama = "";
    }

    // ── Preview ─────────────────────────────────────────────────────────

    private async Task PreviewYap()
    {
        previewYukleniyor = true;
        hataMesaji = null;
        preview = null;
        hesaplamaSonucu = null;

        try
        {
            preview = await PreviewEngine.PreviewAsync(seciliYil, seciliAy, seciliKurumId);
        }
        catch (Exception ex)
        {
            hataMesaji = ex.Message;
        }
        finally
        {
            previewYukleniyor = false;
        }
    }

    // ── Hesapla ─────────────────────────────────────────────────────────

    private async Task Hesapla()
    {
        if (preview == null || !preview.HesaplamaYapilabilir) return;

        hesaplamaYapiliyor = true;
        hataMesaji = null;
        sonucMesaji = null;

        try
        {
            hesaplamaSonucu = await Engine.ProcessDonemAsync(
                seciliYil, seciliAy, seciliKurumId,
                hesaplayan: "UI", // TODO: gerçek kullanıcı adı
                notlar: preview.RevizyonYapilacak ? $"Revizyon: V{preview.YeniVersiyon}" : null);

            sonucMesaji = preview.RevizyonYapilacak
                ? $"Revizyon tamamlandı. V{preview.OncekiVersiyon} → V{preview.YeniVersiyon}. {hesaplamaSonucu.UretilenPuantajKayit} kayıt, {hesaplamaSonucu.SupersededKayit} superseded."
                : $"Hesaplama tamamlandı. {hesaplamaSonucu.UretilenPuantajKayit} puantaj kaydı oluşturuldu.";

            ToastService.ShowSuccess(sonucMesaji);
            preview = null; // refresh için temizle
        }
        catch (Exception ex)
        {
            hataMesaji = ex.Message;
            ToastService.ShowError("Hesaplama hatası: " + ex.Message);
        }
        finally
        {
            hesaplamaYapiliyor = false;
        }
    }

    // ── Comparison ──────────────────────────────────────────────────────

    private async Task Karsilastir()
    {
        if (preview?.OncekiHesapDonemiId == null || hesaplamaSonucu == null) return;

        comparisonYukleniyor = true;
        try
        {
            comparison = await PreviewEngine.CompareAsync(
                preview.OncekiHesapDonemiId.Value, hesaplamaSonucu.HesapDonemiId);
        }
        catch (Exception ex)
        {
            hataMesaji = ex.Message;
        }
        finally
        {
            comparisonYukleniyor = false;
        }
    }

    // ── Drill-Down ──────────────────────────────────────────────────────

    private async Task DrillDownAc(PreviewGrupDetay grup)
    {
        drillDownYukleniyor = true;
        drillDownBaslik = $"{grup.GuzergahAdi} / {grup.Plaka} / {grup.Slot}";
        try
        {
            drillDownOps = await PreviewEngine.DrillDownAsync(
                grup.GuzergahId, grup.AracId,
                (int)Enum.Parse<SeferSlot>(grup.Slot),
                seciliYil, seciliAy);
            drillDownAcik = true;
        }
        catch (Exception ex)
        {
            hataMesaji = ex.Message;
        }
        finally
        {
            drillDownYukleniyor = false;
        }
    }

    private void DrillDownKapat()
    {
        drillDownAcik = false;
    }

    // ── Onay ──────────────────────────────────────────────────────────

    private async Task FinansOnayla()
    {
        if (hesaplamaSonucu == null) return;
        onayYukleniyor = true;
        try
        {
            await Workflow.FinansOnaylaAsync(hesaplamaSonucu.HesapDonemiId, "UI");
            ToastService.ShowSuccess("Finans onayı verildi.");
            await LoadAuditLogs();
        }
        catch (Exception ex) { hataMesaji = ex.Message; }
        finally { onayYukleniyor = false; }
    }

    private async Task MuhasebeOnayla()
    {
        if (hesaplamaSonucu == null) return;
        onayYukleniyor = true;
        try
        {
            await Workflow.MuhasebeOnaylaAsync(hesaplamaSonucu.HesapDonemiId, "UI");
            ToastService.ShowSuccess("Muhasebe onayı verildi.");
            await LoadAuditLogs();
        }
        catch (Exception ex) { hataMesaji = ex.Message; }
        finally { onayYukleniyor = false; }
    }

    private async Task Kilitle()
    {
        if (hesaplamaSonucu == null) return;
        onayYukleniyor = true;
        try
        {
            await Workflow.KilitleAsync(hesaplamaSonucu.HesapDonemiId, "UI");
            ToastService.ShowSuccess("Dönem kilitlendi.");
            await LoadAuditLogs();
        }
        catch (Exception ex) { hataMesaji = ex.Message; }
        finally { onayYukleniyor = false; }
    }

    private async Task LoadAuditLogs()
    {
        if (hesaplamaSonucu == null) return;
        auditLogs = await Workflow.GetAuditLogsAsync(hesaplamaSonucu.HesapDonemiId);
    }

    // ── Finansal ──────────────────────────────────────────────────────

    private async Task FinansalKayitOlustur()
    {
        if (hesaplamaSonucu == null) return;
        finansYukleniyor = true;
        try
        {
            await FinansService.FinansalKayitOlusturAsync(hesaplamaSonucu.HesapDonemiId);
            ToastService.ShowSuccess("Finansal kayıtlar oluşturuldu.");
            finansalKayitlar = await FinansService.FinansalKayitlariGetirAsync(hesaplamaSonucu.HesapDonemiId);
        }
        catch (Exception ex) { hataMesaji = ex.Message; }
        finally { finansYukleniyor = false; }
    }

    private async Task TopluFaturaUret()
    {
        if (hesaplamaSonucu == null) return;
        finansYukleniyor = true;
        try
        {
            finansalUretilenFaturaAdet = await FinansService.TopluFaturaUretAsync(hesaplamaSonucu.HesapDonemiId);
            ToastService.ShowSuccess($"{finansalUretilenFaturaAdet} finansal kayıt için fatura üretildi.");
            finansalKayitlar = await FinansService.FinansalKayitlariGetirAsync(hesaplamaSonucu.HesapDonemiId);
        }
        catch (Exception ex) { hataMesaji = ex.Message; }
        finally { finansYukleniyor = false; }
    }
}
