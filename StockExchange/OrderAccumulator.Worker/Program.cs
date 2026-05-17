var builder = Host.CreateApplicationBuilder();
var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()!;

builder.Logging.AddConsole();
builder.Services.AddSingleton<IApplication, AcceptorApplication>();
builder.Services.AddQuickFIXConfiguration(appSettings.PathFileConfigurationQuickFIX);
builder.Services.AddSingleton<IAcceptor>(serviceProvider => 
{
    var application = serviceProvider.GetRequiredService<IApplication>();
    var messageStoreFactory = serviceProvider.GetRequiredService<IMessageStoreFactory>();
    var sessionSettings = serviceProvider.GetRequiredService<SessionSettings>();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    
    return new ThreadedSocketAcceptor(application, messageStoreFactory, sessionSettings, loggerFactory);
});
builder.Services.AddSingleton<IOrderAccumulatorService, OrderAccumulatorService>();
builder.Services.AddHostedService<OrderAccumulatorWorker>();

await builder.Build()
    .RunAsync();