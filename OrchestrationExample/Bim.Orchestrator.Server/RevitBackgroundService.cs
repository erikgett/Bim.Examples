using Bim.Library.ProgramsRunner;

namespace Bim.Orchestrator.Server;

public class RevitBackgroundService : BackgroundService
{
    private readonly int revitInstanceCount;
    private readonly string journalPath;
    private readonly List<RevitRunner> revitRunners = new();

    public RevitBackgroundService(IConfiguration configuration)
    {
        var section = configuration.GetSection("RevitOrchestration");
        journalPath = section["JournalPath"] ?? throw new ArgumentNullException("JournalPath is not set.");
        revitInstanceCount = int.TryParse(section["InstanceCount"], out var count) ? count : 3;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        for (int i = 0; i < revitInstanceCount; i++)
        {
            var journalFile = Path.Combine(journalPath, $"Runner.txt");
            var runner = new RevitRunner(journalFile);
            revitRunners.Add(runner);
            Task.Run(() => runner.Run(), stoppingToken);
        }

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var runner in revitRunners)
        {
            runner.Kill();
        }
        return base.StopAsync(cancellationToken);
    }
}
