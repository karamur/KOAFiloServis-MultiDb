using KOAFiloServis.Web.Services.Interfaces;
using Quartz;

namespace KOAFiloServis.Web.Jobs;

[DisallowConcurrentExecution]
public class PuantajEngineJob : IJob
{
    private readonly IPuantajJobService _jobService;

    public PuantajEngineJob(IPuantajJobService jobService)
    {
        _jobService = jobService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var yil = now.Month == 1 ? now.Year - 1 : now.Year;
        var ay = now.Month == 1 ? 12 : now.Month - 1;

        await _jobService.ProcessAllTenantsAsync(yil, ay, "Quartz", context.CancellationToken);
    }
}
