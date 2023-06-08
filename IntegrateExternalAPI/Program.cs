using IntegrateExternalAPI.Repository;
using IntegrateExternalAPI.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using IntegrateExternalAPI.Data;
using IntegrateExternalAPI.Contracts;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
namespace IntegrateExternalAPI
{
    static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                string filePath = Path.Combine(Directory.GetCurrentDirectory(),"nlog.config");
                LogManager.LoadConfiguration(filePath);
                var serviceProvider = services.BuildServiceProvider();
                IExternalApiService? apiClient = serviceProvider.GetService<IExternalApiService>();
                if (apiClient != null)
                {
                    await apiClient.ExternalService();
                }
                else
                {
                    System.Console.WriteLine("IExternalApiService is not registered in DI container.");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error occurred while Processing the data {ex}");
            }

        }
        /// <summary>
        /// The function sets up a dependency injection container and configures various services such as
        /// HttpClient, MemoryCache, and Logging for an external API.
        /// </summary>
        /// <param name="IServiceCollection">An interface for defining a collection of service
        /// descriptors, which are used to register services with the dependency injection
        /// container.</param>
        /// <returns>
        /// The method `ConfigureServices` is not returning anything. It is setting up the dependency
        /// injection container by adding various services to it.
        /// </returns>
        private static void ConfigureServices(IServiceCollection services)// sets up the dependency injection container.
        {
            services.AddHttpClient();
            services.AddTransient<HttpClient>(s =>
            {
                var configuration = s.GetRequiredService<IConfiguration>();
                var baseUri = configuration.GetValue<string>("MyApiSettings:BaseAddress");
                var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseUri);
                var accessToken = configuration.GetValue<string>("MyApiSettings:AccessToken");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return httpClient;
            });
            services.AddMemoryCache();
            //Logging service
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                loggingBuilder.AddNLog();
            });
            services.AddScoped<IExternalApiRepository, ExternalApiRepository>();
            services.AddScoped<IExternalApiService, ExternalApiService>();
            services.AddScoped<IApiClient, ApiClient>();
            services.AddSingleton<IExportToExcel, ExportToExcel>();
            IConfigurationBuilder builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            IConfiguration config = builder.Build();
            services.AddSingleton<IConfiguration>(config);

        }
    }
}
