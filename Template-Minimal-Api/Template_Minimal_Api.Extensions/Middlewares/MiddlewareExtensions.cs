namespace Template.MinimalApi.Extensions.Middlewares;

public static class MiddlewareExtensions
{
    public static IServiceCollection AddGlobalExceptionHandlerMiddleware(this IServiceCollection services)
    {
        //services.AddTransient<GlobalExceptionHandlerMiddleware>();

        services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
        services.AddProblemDetails();

        services.AddTransient<UnauthorizedTokenMiddleware>();

        return services;
    }
}
