using Microsoft.Extensions.Logging;
using IntegrateExternalAPI.Contracts;
namespace IntegrateExternalAPI.Repository
{
    public class ExternalApiRepository : IExternalApiRepository
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<ExternalApiRepository> _logger;

        public ExternalApiRepository(IApiClient apiClient, ILogger<ExternalApiRepository> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        /// <summary>
        /// This function retrieves a list of objects of a specified type from a specified API endpoint
        /// and logs any errors that occur.
        /// </summary>
        /// <param name="endPoint">A string parameter representing the endpoint of the API to be
        /// called.</param>
        /// <returns>
        /// A list of objects of type T.
        /// </returns>
        public async Task<List<T>> GetAsync<T>(string endPoint)
        {
            var result = await _apiClient.GetList<T>($"/public/v2/{endPoint}");
            return result;
        }

        /// <summary>
        /// This is a generic function that retrieves an object of type T by its ID from a specified API
        /// endpoint.
        /// </summary>
        /// <param name="Id">The unique identifier of the object that needs to be retrieved from the
        /// API.</param>
        /// <param name="endPoint">The endpoint is a string parameter that represents the specific API
        /// endpoint that the method is calling. It is used to construct the URL for the API
        /// request.</param>
        /// <returns>
        /// A generic type T is being returned, which is obtained by making a GET request to a specific
        /// endpoint with a given ID.
        /// </returns>
        public async Task<T> GetByIdAsync<T>(long Id, string endPoint)
        {
            T result = await _apiClient.Get<T>($"/public/v2/{endPoint}/{Id}");
            return result;
        }

        /// <summary>
        /// This C# function retrieves a list of objects of type T by their ID from a specified API
        /// endpoint and resource.
        /// </summary>
        /// <param name="Id">A long integer representing the unique identifier of the resource being
        /// requested.</param>
        /// <param name="endPoint">The endpoint is a string that represents the specific API endpoint that
        /// the method is trying to access. It is used to construct the URL that will be used to make the
        /// API call.</param>
        /// <param name="resource">The name of the resource that we want to retrieve from the API.</param>
        /// <returns>
        /// A list of objects of type T, retrieved from the specified endpoint, resource and with the
        /// specified Id.
        /// </returns>
        public async Task<List<T>> GetByIdAsync<T>(long Id, string endPoint, string resource)
        {
            var result = await _apiClient.GetList<T>($"/public/v2/{endPoint}/{Id}/{resource}");
            return result;

        }
        public async Task<T> PostAsync<T>(T data, long Id, string endPoint, string resource)
        {
            T result = await _apiClient.PostData<T, T>($"/public/v2/{endPoint}/{Id}/{resource}", data);
            _logger.LogInformation($"Successfully created list of Post with ID {Id} from endpoint {endPoint} and resource {resource}");
            return result;
        }


    }
}
