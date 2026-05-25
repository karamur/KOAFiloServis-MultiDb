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
}
