var builder = Host.CreateApplicationBuilder();
var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()!;

builder.Logging.AddConsole();
builder.Services.AddSingleton<IApplication, AcceptorApplication>();
builder.Services.AddQuickFIXConfiguration<IApplication>(appSettings.PathFileConfigurationQuickFIX, 
    isAcceptor: appSettings.IsAcceptor());
builder.Services.AddSingleton<IOrderAccumulatorService, OrderAccumulatorService>();
builder.Services.AddHostedService<OrderAccumulatorWorker>();

await builder.Build()
    .RunAsync();