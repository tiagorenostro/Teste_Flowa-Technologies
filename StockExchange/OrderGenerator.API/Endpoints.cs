namespace OrderGenerator.API;

public static class Endpoints
{
    private const string PrefixApi = "api";

    public static WebApplication AddEndpoints(this WebApplication app)
    {
        var route = app.MapGroup(PrefixApi);
        route.AddRoutePing();
        route.AddRouteOrder();
        route.AddRouteShare();
        return app;
    }
}