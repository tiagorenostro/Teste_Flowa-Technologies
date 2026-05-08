namespace OrderGenerator.API.Routes;

public static class RouteShare
{
    public static void AddRouteShare(this RouteGroupBuilder routeGroupBuilder)
    {
        var routeGroup = routeGroupBuilder.MapGroup("share");

        routeGroup.MapGet("", GetShare);
    }

    private static IResult GetShare([FromServices] IShareService shareService) =>
        TypedResults.Ok(shareService.GetShares());
}