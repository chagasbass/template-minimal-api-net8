﻿namespace Template.MinimalApi.Extensions.Resiliences
{
    public static class ResiliencePolicies
    {
        public static IAsyncPolicy<HttpResponseMessage> GetApiRetryPolicy(int quantidadeDeRetentativas, IServiceProvider serviceProvider)
        {
            var quantidadeTotalDeRetentativas = quantidadeDeRetentativas;

            const string _retryMessage = "Retentativas de chamadas externas foram excedidas.";

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode != HttpStatusCode.OK)
                .RetryAsync(quantidadeDeRetentativas, onRetry: (message, numeroDeRetentativas) =>
                {
                    if (quantidadeTotalDeRetentativas == numeroDeRetentativas && message.Result is not null)
                    {
                        var logServices = serviceProvider.GetService<ILogServices>();

                        logServices.LogData.AddMessageInformation(_retryMessage)
                                       .AddResponseStatusCode((int)message.Result.StatusCode)
                                       .AddRequestUrl(message.Result.RequestMessage.RequestUri.AbsoluteUri)
                                       .AddRequestBody(message.Result.RequestMessage.Content)
                                       .AddResponseBody(message.Result.Content);

                        logServices.WriteLogFromResiliences();
                    }
                });
        }
    }
}