using CurrencyConversion.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversion.Infrastructure.Provider
{
    /// <summary>
    /// CurrencyProviderFactory
    /// </summary>
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, Type> providers;
        public CurrencyProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            this.serviceProvider = serviceProvider;
            this.configuration = configuration;
            providers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
                {
                    { "frankfurter", typeof(FrankfurterCurrencyProvider) }
                    // Add more providers here as needed
                };
        }

        public ICurrencyProvider GetProvider()
        {
            var providerName = configuration["ExchangeRateProvider"] ?? "Frankfurter";
            if (providers.TryGetValue(providerName, out var providerType))
            {
               var prov= serviceProvider.GetRequiredService<FrankfurterCurrencyProvider>();
                return prov;
            }

            throw new NotImplementedException($"Provider '{providerName}' not supported.");
        }
    }
}
