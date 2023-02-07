using Proto;
using Proto.Cluster;

namespace GrainTest.Actors;

public sealed class TestGrain : TestGrainBase
{
    private readonly ILogger<TestGrain> _logger;
    private readonly IContext _context;
    private readonly ClusterIdentity _clusterIdentity;

    public TestGrain(ILogger<TestGrain> logger, IContext context, ClusterIdentity clusterIdentity)
        : base(context)
    {
        _logger = logger;
        _context = context;
        _clusterIdentity = clusterIdentity;
        
        _logger.LogInformation("{Identity} created", _clusterIdentity.Identity);
    }

    public override async Task Test()
    {
        _logger.LogInformation("{Identity}: Test called", _clusterIdentity.Identity);
    }
}