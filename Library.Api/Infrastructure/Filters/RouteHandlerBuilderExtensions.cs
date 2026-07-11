namespace Library.Api.Infrastructure.Filters;

// Keeps endpoint validation wiring readable once routes start accepting DTOs.
public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder Validate<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : class
    {
        return builder.AddEndpointFilter<ValidationFilter<TRequest>>();
    }
}
