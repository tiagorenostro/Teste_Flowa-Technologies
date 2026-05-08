var builder = Host.CreateApplicationBuilder();

var configuracao = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()!;

builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddSingleton<IApplication, AcceptorApplication>();
builder.Services.AddSingleton(new SessionSettings(Path.Combine(AppContext.BaseDirectory, configuracao.PathFileConfigurationQuickFIX)));
builder.Services.AddSingleton<IMessageStoreFactory, FileStoreFactory>();
builder.Services.AddSingleton<IAcceptor>(sp => 
{
    var app = sp.GetRequiredService<IApplication>();
    var storeFactory = sp.GetRequiredService<IMessageStoreFactory>();
    var sessionSettings = sp.GetRequiredService<SessionSettings>();
    var logger = sp.GetRequiredService<ILoggerFactory>();
    
    return new ThreadedSocketAcceptor(app, storeFactory, sessionSettings, logger);
});

builder.Services.AddSingleton<IOrderAccumulatorService, OrderAccumulatorService>();
builder.Services.AddHostedService<OrderAccumulatorWorker>();

await builder.Build()
    .RunAsync();