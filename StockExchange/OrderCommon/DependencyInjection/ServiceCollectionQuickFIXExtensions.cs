namespace OrderCommon.DependencyInjection;

public static class ServiceCollectionQuickFIXExtensions
{
    public static void AddQuickFIXConfiguration<T>(this IServiceCollection services, string path, bool isAcceptor) where T : class, IApplication
    {
        services.AddSingleton<IMessageStoreFactory, FileStoreFactory>();
        services.AddSingleton(_ => new SessionSettings(Path.Combine(AppContext.BaseDirectory, path)));
        
        AddComunicationSocket<T>(services, isAcceptor);
    }

    private static void AddComunicationSocket<T>(IServiceCollection services, bool isAcceptor) where T : class, IApplication
    {
        if (isAcceptor)
            services.AddSingleton<IAcceptor>(serviceProvider => CreateConnector<T, ThreadedSocketAcceptor>(serviceProvider, 
                (app, store, settings, log) => 
                new ThreadedSocketAcceptor(app, store, settings, log)));
        else
            services.AddSingleton<IInitiator>(serviceProvider => CreateConnector<T, QuickFix.Transport.SocketInitiator>(serviceProvider, 
                (app, store, settings, log) => 
                new QuickFix.Transport.SocketInitiator(app, store, settings, log)));
    }

    private static TConnector CreateConnector<T, TConnector>(
        IServiceProvider serviceProvider, 
        Func<IApplication, IMessageStoreFactory, SessionSettings, ILoggerFactory, TConnector> factory) 
        where T : class, IApplication
    {
        var application = serviceProvider.GetRequiredService<T>();
        var messageStoreFactory = serviceProvider.GetRequiredService<IMessageStoreFactory>();
        var sessionSettings = serviceProvider.GetRequiredService<SessionSettings>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        return factory(application, messageStoreFactory, sessionSettings, loggerFactory);
    }
}