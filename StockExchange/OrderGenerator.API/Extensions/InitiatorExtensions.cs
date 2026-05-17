namespace OrderGenerator.API.Extensions;

public static class InitiatorExtensions
{
    public static void InitiateCommunication(this WebApplication webApplication) =>
        webApplication.Services
            .GetRequiredService<IInitiator>()
            .Start();
}