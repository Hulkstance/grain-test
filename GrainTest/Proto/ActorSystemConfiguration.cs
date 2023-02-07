using Google.Protobuf.WellKnownTypes;
using GrainTest.Actors;
using GrainTest.Services;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Cache;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.DependencyInjection;
using Proto.Remote;
using Proto.Remote.GrpcNet;

namespace GrainTest.Proto;

public static class ActorSystemConfiguration
{
    public static IServiceCollection AddActorSystem(this IServiceCollection services, bool developerLogging = true)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(provider =>
        {
            const string clusterName = "TestCluster";
            
            Log.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());

            var actorSystemConfig = ActorSystemConfig
                .Setup()
                .WithDeadLetterThrottleCount(3)
                .WithDeadLetterThrottleInterval(TimeSpan.FromSeconds(1));

            if (developerLogging)
            {
                actorSystemConfig
                    .WithDeveloperSupervisionLogging(true)
                    .WithDeadLetterRequestLogging(true)
                    .WithDeveloperThreadPoolStatsLogging(true);
            }

            var remoteConfig = GrpcNetRemoteConfig
                .BindToLocalhost()
                .WithProtoMessages(MessagesReflection.Descriptor);

            var clusterConfig = ClusterConfig
                .Setup(
                    clusterName: clusterName,
                    clusterProvider: new TestProvider(new TestProviderOptions(), new InMemAgent()),
                    identityLookup: new PartitionIdentityLookup()
                )
                .WithClusterKind(
                    kind: TestGrainActor.Kind,
                    prop: Props.FromProducer(() =>
                        new TestGrainActor((context, clusterIdentity) =>
                            ActivatorUtilities.CreateInstance<TestGrain>(provider, context, clusterIdentity)
                        )
                    )
                );
            
            return new ActorSystem(actorSystemConfig)
                .WithServiceProvider(provider)
                .WithRemote(remoteConfig)
                .WithCluster(clusterConfig);
        });

        services.AddSingleton(provider => provider.GetRequiredService<ActorSystem>().Cluster());
        services.AddHostedService<ActorSystemHostedService>();
        services.AddHostedService<ActorSpawnerHostedService>();

        return services;
    }
}
