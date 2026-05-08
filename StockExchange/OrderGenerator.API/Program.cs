var builder = WebApplication.CreateBuilder();

var configuracao = builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()!;

builder.Services.AddLogging(logging => logging.AddConsole());
builder.Services.AddSingleton<IInitiatorApplication, InitiatorApplication>();
builder.Services.AddSingleton(new SessionSettings(Path.Combine(AppContext.BaseDirectory, configuracao.PathFileConfigurationQuickFIX!)));
builder.Services.AddSingleton<IMessageStoreFactory, FileStoreFactory>();
builder.Services.AddSingleton<IInitiator>(sp =>
{
    var application = sp.GetRequiredService<IInitiatorApplication>();
    var storeFactory = sp.GetRequiredService<IMessageStoreFactory>();
    var sessionSettings = sp.GetRequiredService<SessionSettings>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

    return new QuickFix.Transport.SocketInitiator(application, storeFactory, sessionSettings, loggerFactory);
});

builder.Services.AddSingleton<ITradingGateway, TradingGateway>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(configuracao.CorsPolicy!,
        configurePolicy: policy =>
        {
            policy.WithOrigins(configuracao.UrlStockExhangeWeb!)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var app = builder.Build()
    .AddEndpoints();

var socketInitiator = app.Services.GetRequiredService<IInitiator>();
socketInitiator.Start();

app.UseCors(configuracao.CorsPolicy!);
app.MapHub<TradingHub>("/tradingHub");
app.UseExceptionHandler(applicationBuilder =>
{
    applicationBuilder.Run(async context =>
    {
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

        if (contextFeature is not null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(contextFeature.Error, MessageError.ErrorGeneric);
            
            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            await context.Response.WriteAsJsonAsync(new ErrorDto(MessageError.RequestNotProcessed, []), options);
        }
    });
});

await app.RunAsync();