namespace OrderGenerator.API;

public static class Endpoints
{
    private const string PrefixApi = "api";

    public static WebApplication AddEndpoints(this WebApplication webApplication)
    {
        var routeGroupBuilder = webApplication.MapGroup(PrefixApi);
        routeGroupBuilder.AddRoutePing();
        routeGroupBuilder.AddRouteOrder();
        routeGroupBuilder.AddRouteShare();
        
        return webApplication;
    }
}