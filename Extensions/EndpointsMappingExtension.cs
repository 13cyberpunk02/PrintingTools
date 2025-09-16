using PrintingTools.Endpoints;

namespace PrintingTools.Extensions;

public static class EndpointsMappingExtension
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
        endpoints.MapUserEndpoints();
        return endpoints;
    }
}