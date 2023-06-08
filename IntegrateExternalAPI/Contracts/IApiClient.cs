using System.Collections.Generic;

namespace IntegrateExternalAPI.Contracts
{
    public interface IApiClient
    {
        /// <summary>
        /// This function retrieves a list of objects from an API endpoint, caches the response for 10
        /// minutes, and logs any errors that occur.
        /// </summary>
        /// <param name="endpoint">A string representing the API endpoint to call.</param>
        /// <returns>
        /// A generic list of type T, which is obtained either from the cache or by making an HTTP GET
        /// request to the specified endpoint and deserializing the response body to a list of type T.
        /// The response is also cached for 10 minutes using a memory cache. If an exception occurs
        /// during the HTTP request, it is logged and re-thrown as a new exception.
        /// </returns>

        Task<List<T>> GetList<T>(string endpoint);

        /// <summary>
        /// This function retrieves data from a cache or an API endpoint, caches the response, and
        /// returns the data as a generic type.
        /// </summary>
        /// <param name="endpoint">A string representing the API endpoint to retrieve data from.</param>
        /// <returns>
        /// The method is returning an object of type T, which is the deserialized JSON response obtained
        /// from the specified endpoint.
        /// </returns>
        Task<T> Get<T>(string endpoint);

        /// <summary>
        /// This function sends a POST request to a specified endpoint with serialized data and handles
        /// retries and error handling.
        /// </summary>
        /// <param name="endpoint">The URL endpoint to which the POST request will be sent.</param>
        /// <param name="TRequest">The type of the data object being sent in the POST request.</param>
        /// <param name="accessToken">A string representing the access token needed for authentication to
        /// access the endpoint.</param>
        /// <returns>
        /// The method is returning an object of type TResponse, which is the deserialized response data
        /// from the HTTP POST request sent to the specified endpoint. If the request is unsuccessful or
        /// encounters an error, the method returns the default value of TResponse.
        /// </returns>
         Task<TResponse> PostData<TRequest, TResponse>(string endpoint, TRequest data);


    }
}