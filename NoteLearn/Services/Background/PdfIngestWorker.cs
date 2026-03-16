
namespace NoteLearn.Services.Background;

public class PdfIngestWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<PdfIngestWorker> _logger;

    public PdfIngestWorker(IBackgroundTaskQueue queue, ILogger<PdfIngestWorker> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PdfIngestWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background work item failed.");
            }
        }
    }
}
