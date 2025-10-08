using System;
using MessagePipe;
using VContainer;
using VContainer.Unity;

public class MessagePipeLifetimeScope : LifetimeScope
{
    public static event Action OnGlobalMessagePipeSet;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterMessagePipe();
        
        builder.RegisterBuildCallback(resolver =>
        {
            GlobalMessagePipe.SetProvider(resolver.AsServiceProvider());
            OnGlobalMessagePipeSet?.Invoke();
        });

    }
}
