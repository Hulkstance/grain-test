using GrainTest.Models;
using Proto;
using Proto.Cluster;

namespace GrainTest.Actors;

public sealed class LocalActor : IActor
{
    private readonly ILogger<LocalActor> _logger;
    private readonly Cluster _cluster;

    public LocalActor(ILogger<LocalActor> logger, Cluster cluster)
    {
        _logger = logger;
        _cluster = cluster;
    }
    
    public async Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                break;
            
            case DoSomething:
                var testGrainClient = _cluster
                    .GetTestGrain("test");

                _ = testGrainClient.Test(CancellationTokens.WithTimeout(TimeSpan.FromSeconds(5)));
                
                break;
            
            case Stopping:
                break;
        }
    }
}