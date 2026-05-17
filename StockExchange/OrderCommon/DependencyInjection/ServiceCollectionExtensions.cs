namespace OrderCommon.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddQuickFIXConfiguration(this IServiceCollection services, string path)
    {
        services.AddSingleton<IMessageStoreFactory, FileStoreFactory>();
        services.AddSingleton(new SessionSettings(Path.Combine(AppContext.BaseDirectory, path)));
    }
}