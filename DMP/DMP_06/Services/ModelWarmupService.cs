using Microsoft.Extensions.Hosting;

namespace DMP_06.Services;

public class ModelWarmupService : IHostedService
{
    private readonly OnnxPredictor _predictor;

    public ModelWarmupService(OnnxPredictor predictor)
    {
        _predictor = predictor;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // НЕ await — запускаємо у фоні
        _ = Task.Run(() => _predictor.WarmUpAsync(), cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}