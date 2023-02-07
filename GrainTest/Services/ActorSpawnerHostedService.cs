using GrainTest.Actors;
using GrainTest.Models;
using Proto;
using Proto.Mailbox;

namespace GrainTest.Services;

public sealed class ActorSpawnerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private const int RetryAttempts = 5;
    
    private readonly IRootContext _rootContext;

    private PID? _pid;
    
    public ActorSpawnerHostedService(ActorSystem actorSystem, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _rootContext = actorSystem.Root;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var props = Props
            .FromProducer(() => ActivatorUtilities.CreateInstance<LocalActor>(_serviceProvider))
            .WithGuardianSupervisorStrategy(new OneForOneStrategy((_, _) =>
                SupervisorDirective.Restart, RetryAttempts, null))
            .WithDispatcher(new ThreadPoolDispatcher());

        _pid = _rootContext.Spawn(props);
        
        _rootContext.Send(_pid, new DoSomething());
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) 
        => _rootContext.PoisonAsync(_pid!);
}