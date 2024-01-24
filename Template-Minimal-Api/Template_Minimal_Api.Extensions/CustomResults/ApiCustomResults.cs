﻿namespace Template.MinimalApi.Extensions.CustomResults
{
    public class ApiCustomResults : IApiCustomResults
    {
        private readonly ILogServices _logServices;
        private readonly INotificationServices _notificationServices;

        public ApiCustomResults(ILogServices logServices,
                                INotificationServices notificationServices)
        {
            _logServices = logServices;
            _notificationServices = notificationServices;
        }

        public IResult FormatApiResponse(CommandResult commandResult, string? defaultEndpointRoute = null)
        {
            var statusCodeOperation = _notificationServices.StatusCode;

            ICommandResult result = default;

            if (_notificationServices.HasNotifications())
            {
                result = CreateErrorResponse(statusCodeOperation.Id, commandResult);

                _notificationServices.ClearNotifications();
            }

            switch (statusCodeOperation)
            {
                case var _ when statusCodeOperation == StatusCodeOperation.BadRequest:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.BadRequest);
                    return Results.BadRequest(result);
                case var _ when statusCodeOperation == StatusCodeOperation.NotFound:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.NotFound);
                    return Results.NotFound(commandResult);
                case var _ when statusCodeOperation == StatusCodeOperation.Unauthorized:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.Unauthorized);
                    return Results.Unauthorized();
                case var _ when statusCodeOperation == StatusCodeOperation.BusinessError:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.UnprocessableEntity);
                    return Results.UnprocessableEntity(commandResult);
                case var _ when statusCodeOperation == StatusCodeOperation.Created:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.Created);
                    return Results.Created(defaultEndpointRoute, commandResult);
                case var _ when statusCodeOperation == StatusCodeOperation.NoContent:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.NoContent);
                    return Results.NoContent();
                case var _ when statusCodeOperation == StatusCodeOperation.OK:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.OK);
                    return Results.Ok(commandResult);
                case var _ when statusCodeOperation == StatusCodeOperation.Accepted:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.Accepted);
                    return Results.Ok(commandResult);
                default:
                    GenerateLogResponse(commandResult, (int)HttpStatusCode.OK);
                    return Results.Ok(commandResult);
            }
        }

        public void GenerateLogResponse(CommandResult commandResult, int statusCode)
        {
            _logServices.LogData.AddResponseStatusCode(statusCode)
                                .AddResponseBody(commandResult);

            _logServices.WriteLog();
        }

        public ICommandResult CreateErrorResponse(int statusCode, CommandResult commandResult)
        {
            var options = JsonOptionsFactory.GetSerializerOptions();

            var notifications = _notificationServices.GetNotifications();

            var jsonNotifications = JsonSerializer.Serialize(notifications, options);

            var detail = jsonNotifications;
            var defaultTitle = "Um erro ocorreu ao processar o request.";

            var problemDetails = new MinimalApiProblemDetail(notifications.ToList(), commandResult.Message, statusCode, defaultTitle);

            commandResult.Data = problemDetails;

            return commandResult;
        }

    }
}
