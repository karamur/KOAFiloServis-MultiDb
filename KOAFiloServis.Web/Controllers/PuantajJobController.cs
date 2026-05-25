using System.Security.Claims;
using KOAFiloServis.Shared.Entities;
using KOAFiloServis.Web.Data;
using KOAFiloServis.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KOAFiloServis.Web.Controllers;

[ApiController]
[Route("api/puantaj/jobs")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,Muhasebeci")]
public class PuantajJobController : ControllerBase
{
    private readonly IPuantajJobService _jobService;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public PuantajJobController(
        IPuantajJobService jobService,
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _jobService = jobService;
        _dbFactory = dbFactory;
    }

    [HttpPost("process/{yil:int}/{ay:int}")]
    public async Task<IActionResult> ProcessAll(int yil, int ay, CancellationToken ct)
    {
        var tetikleyen = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub") ?? "Manuel";
        try
        {
            var result = await _jobService.ProcessAllTenantsAsync(yil, ay, tetikleyen, ct);
            return Ok(new
            {
                result.Durum,
                DurumAdi = result.Durum.ToString(),
                Basarili = result.IslenenOperasyon,
                Atlanan = result.UretilenPuantaj,
                Message = $"{yil}/{ay} işlendi: {result.Durum}"
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { Error = "İşlem iptal edildi." });
        }
    }

    [HttpPost("process/{firmaId:int}/{yil:int}/{ay:int}")]
    public async Task<IActionResult> ProcessTenant(int firmaId, int yil, int ay, CancellationToken ct)
    {
        var tetikleyen = User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub") ?? "Manuel";
        try
        {
            await _jobService.ProcessTenantAsync(firmaId, null, yil, ay, tetikleyen, ct);
            return Ok(new { Message = $"Firma {firmaId} {yil}/{ay} işlendi." });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { Error = "İşlem iptal edildi." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int count = 50, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var jobs = await db.PuantajJobExecutions
            .Where(j => !j.IsDeleted)
            .OrderByDescending(j => j.CreatedAt)
            .Take(Math.Min(count, 100))
            .Select(j => new
            {
                j.Id, j.FirmaId, j.Yil, j.Ay, j.Tetikleyen,
                Durum = (int)j.Durum,
                j.Baslangic, j.Bitis, j.Versiyon, j.HesapDonemiId,
                j.IslenenOperasyon, j.UretilenPuantaj,
                j.HataMesaji
            })
            .ToListAsync(ct);

        var result = jobs.Select(j => new PuantajJobHistoryDto
        {
            Id = j.Id,
            FirmaId = j.FirmaId,
            Yil = j.Yil,
            Ay = j.Ay,
            Tetikleyen = j.Tetikleyen,
            Durum = ((PuantajJobExecutionDurum)j.Durum).ToString(),
            Baslangic = j.Baslangic,
            Bitis = j.Bitis,
            SureSn = j.Baslangic.HasValue && j.Bitis.HasValue
                ? (int?)(j.Bitis!.Value - j.Baslangic!.Value).TotalSeconds
                : null,
            Versiyon = j.Versiyon,
            HesapDonemiId = j.HesapDonemiId,
            IslenenOperasyon = j.IslenenOperasyon,
            UretilenPuantaj = j.UretilenPuantaj,
            HataMesaji = j.HataMesaji != null && j.HataMesaji.Length > 200
                ? j.HataMesaji.Substring(0, 200) + "..."
                : j.HataMesaji
        }).ToList();

        return Ok(result);
    }

    [HttpGet("history/{id:int}")]
    public async Task<IActionResult> GetHistoryDetail(int id, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var job = await db.PuantajJobExecutions
            .Where(j => j.Id == id && !j.IsDeleted)
            .Select(j => new
            {
                j.Id, j.FirmaId, j.Yil, j.Ay, j.Tetikleyen,
                Durum = (int)j.Durum,
                j.Baslangic, j.Bitis, j.Versiyon, j.HesapDonemiId,
                j.IslenenOperasyon, j.UretilenPuantaj, j.HataMesaji
            })
            .FirstOrDefaultAsync(ct);

        if (job == null) return NotFound(new { Error = "Execution kaydı bulunamadı." });

        return Ok(new PuantajJobHistoryDto
        {
            Id = job.Id,
            FirmaId = job.FirmaId,
            Yil = job.Yil,
            Ay = job.Ay,
            Tetikleyen = job.Tetikleyen,
            Durum = ((PuantajJobExecutionDurum)job.Durum).ToString(),
            Baslangic = job.Baslangic,
            Bitis = job.Bitis,
            SureSn = job.Baslangic.HasValue && job.Bitis.HasValue
                ? (int?)(job.Bitis!.Value - job.Baslangic!.Value).TotalSeconds
                : null,
            Versiyon = job.Versiyon,
            HesapDonemiId = job.HesapDonemiId,
            IslenenOperasyon = job.IslenenOperasyon,
            UretilenPuantaj = job.UretilenPuantaj,
            HataMesaji = job.HataMesaji
        });
    }
}

public class PuantajJobHistoryDto
{
    public int Id { get; set; }
    public int? FirmaId { get; set; }
    public int Yil { get; set; }
    public int Ay { get; set; }
    public string Tetikleyen { get; set; } = "";
    public string Durum { get; set; } = "";
    public DateTime? Baslangic { get; set; }
    public DateTime? Bitis { get; set; }
    public int? SureSn { get; set; }
    public int Versiyon { get; set; }
    public int? HesapDonemiId { get; set; }
    public int IslenenOperasyon { get; set; }
    public int UretilenPuantaj { get; set; }
    public string? HataMesaji { get; set; }
}
