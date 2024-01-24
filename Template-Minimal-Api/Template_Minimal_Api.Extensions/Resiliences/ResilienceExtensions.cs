namespace Template.MinimalApi.Extensions.Resiliences
{
    public static class ResilienceExtensions
    {
        public static IServiceCollection AddApiResiliencePatterns(this IServiceCollection services, IConfiguration configuration)
        {
            var quantidadeDeRetentativas = Int32.Parse(configuration["ResilienceConfiguration:QuantidadeDeRetentativas"]);
            var nomeCliente = configuration["ResilienceConfiguration:NomeCliente"];
            var existeServico = bool.Parse(configuration["ResilienceConfiguration:ExisteServico"]);

            var serviceProvider = services.BuildServiceProvider();

            if (quantidadeDeRetentativas > 0)
            {
                services.AddHttpClient(nomeCliente)
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                    .AddPolicyHandler(ResiliencePolicies.GetApiRetryPolicy(quantidadeDeRetentativas, serviceProvider));
            }

            if (existeServico)
            {
                services.AddHttpClient(nomeCliente)
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
            }

            return services;
        }
    }

}
