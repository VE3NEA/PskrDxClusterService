namespace VE3NEA.PskrDxClusterService
{
  public class WindowsBackgroundService : BackgroundService
  {
    private readonly PskrDxClusterServer server = new();

    public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await server.RunAsync(stoppingToken);
    }
  }
}