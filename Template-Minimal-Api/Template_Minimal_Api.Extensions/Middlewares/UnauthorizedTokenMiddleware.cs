﻿namespace Template.MinimalApi.Extensions.Middlewares
{
    public class UnauthorizedTokenMiddleware : IMiddleware
    {
        private readonly ProblemDetailConfigurationOptions _problemOptions;
        private readonly ILogServices _logServices;

        public UnauthorizedTokenMiddleware(IOptions<ProblemDetailConfigurationOptions> options,
                                               ILogServices logServices)
        {
            _problemOptions = options.Value;
            _logServices = logServices;
        }

        private ProblemDetails ConfigureProblemDetails(int statusCode, HttpContext context)
        {
            var defaultTitle = "A Validação do Token de acesso Falhou";
            var defaultDetail = "Acesso Negado";

            var title = _problemOptions.Title;
            var detail = _problemOptions.Detail;
            var instance = context.Request.HttpContext.Request.Path.ToString();

            var type = StatusCodeOperation.RetrieveStatusCode(statusCode);

            if (string.IsNullOrEmpty(title))
                title = defaultTitle;

            if (string.IsNullOrEmpty(detail))
                detail = defaultDetail;

            return new ProblemDetails()
            {
                Detail = detail,
                Instance = instance,
                Status = statusCode,
                Title = title,
                Type = type.Text
            };
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);

            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                const string dataType = @"application/problem+json";
                const int statusCode = StatusCodes.Status401Unauthorized;

                var problemDetails = ConfigureProblemDetails(statusCode, context);

                var commandResult = new CommandResult(problemDetails);

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = dataType;

                _logServices.LogData.AddResponseStatusCode(statusCode)
                                    .AddResponseBody(commandResult)
                                    .AddRequestUrl(context.Request.Path)
                                    .AddRequestQuery(context.Request.QueryString.Value);

                _logServices.WriteLog();

                await context.Response.WriteAsync(JsonSerializer.Serialize(commandResult, JsonOptionsFactory.GetSerializerOptions()));
            }
        }
    }
}
