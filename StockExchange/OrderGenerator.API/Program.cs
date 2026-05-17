var builder = WebApplication.CreateBuilder();
var appSettings = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()!;

builder.Logging.AddConsole();
builder.Services.AddSingleton<IInitiatorApplication, InitiatorApplication>();
builder.Services.AddQuickFIXConfiguration(appSettings.PathFileConfigurationQuickFIX!);
builder.Services.AddSingleton<IInitiator>(serviceProvider =>
{
    var initiatorApplication = serviceProvider.GetRequiredService<IInitiatorApplication>();
    var messageStoreFactory = serviceProvider.GetRequiredService<IMessageStoreFactory>();
    var sessionSettings = serviceProvider.GetRequiredService<SessionSettings>();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

    return new QuickFix.Transport.SocketInitiator(initiatorApplication, messageStoreFactory, sessionSettings, loggerFactory);
});
builder.Services.AddSingleton<ITradingGateway, TradingGateway>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(appSettings.CorsPolicy!,
        configurePolicy: policy =>
        {
            policy.WithOrigins(appSettings.UrlStockExhangeWeb!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var app = builder.Build()
    .AddEndpoints();

app.UseCors(appSettings.CorsPolicy!);
app.MapHub<TradingHub>(appSettings.PatternHub!);
app.UseExceptionHandler();

app.InitiateCommunication();

await app.RunAsync();