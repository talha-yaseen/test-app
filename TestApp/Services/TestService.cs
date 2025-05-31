namespace TestApp.Services;

public class TestService : BackgroundService
{
    private PeriodicTimer _timer;
    public TestService()
    {
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(2));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested && await _timer.WaitForNextTickAsync())
        {
            Console.WriteLine("Testing");
        }
    }
}

