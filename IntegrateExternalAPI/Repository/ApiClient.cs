using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IntegrateExternalAPI.Contracts;
namespace IntegrateExternalAPI.Repository
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ApiClient> _logger;


        public ApiClient(HttpClient client, IMemoryCache cache, ILogger<ApiClient> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }



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
        public async Task<List<T>> GetList<T>(string endpoint)
        {
            try
            {
                if (_cache.TryGetValue(endpoint, out List<T> cachedResponse))
                {
                    _logger.LogInformation($"Retrieved cached response for {endpoint}");
                    return cachedResponse;
                }
                HttpResponseMessage response = await _client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                //Reading Response Body as String 
                string responseBody = await response.Content.ReadAsStringAsync();

                //Convert the string Response to Object
                List<T> returnList = DeserializeJsonToList<T>(responseBody);

                //Adding this response to cache 
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                  .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                _cache.Set(endpoint, returnList, cacheEntryOptions);

                // Cache the response for 10 minutes
                return returnList;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"An HTTP request exception occurred while calling {endpoint}: {ex.Message}");
                throw new HttpRequestException($"Failed to make HTTP request to {endpoint}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// This function deserializes a JSON string into a list of objects of a specified type.
        /// </summary>
        /// <param name="json">a string containing JSON data that needs to be deserialized into a list of
        /// objects of type T.</param>
        /// <returns>
        /// A list of objects of type T, deserialized from a JSON string.
        /// </returns>
        public List<T> DeserializeJsonToList<T>(string json)
        {
            JArray jsonArray = JArray.Parse(json);
            List<T> list = jsonArray.ToObject<List<T>>();
            return list;
        }

        /// <summary>
        /// This function retrieves data from a cache or an API endpoint, caches the response, and
        /// returns the data as a generic type.
        /// </summary>
        /// <param name="endpoint">A string representing the API endpoint to retrieve data from.</param>
        /// <returns>
        /// The method is returning an object of type T, which is the deserialized JSON response obtained
        /// from the specified endpoint.
        /// </returns>
        public async Task<T> Get<T>(string endpoint)
        {
            try
            {
                if (_cache.TryGetValue(endpoint, out T cachedResponse))
                {
                    _logger.LogInformation($"Retrieved cached response for {endpoint}");
                    return cachedResponse;
                }

                HttpResponseMessage response = await _client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                //Reading Response Body as String 
                string responseBody = await response.Content.ReadAsStringAsync();

                //Convert the string Response to Object
                T returnList = DeserializeJson<T>(responseBody);

                //Adding this response to cache 
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                  .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                _cache.Set(endpoint, returnList, cacheEntryOptions);

                return returnList;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"An HTTP request exception occurred while calling {endpoint}: {ex.Message}");
                throw new HttpRequestException($"Failed to make HTTP request to {endpoint}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// This function deserializes a JSON string into an object of the specified type.
        /// </summary>
        /// <param name="json">a string containing JSON data to be deserialized</param>
        /// <returns>
        /// The method is returning an object of type T, which is the deserialized JSON object.
        /// </returns>
        public T DeserializeJson<T>(string json)
        {
            T? item = default(T);
            if (json.StartsWith("["))
            {
                JArray jsonArray = JArray.Parse(json);
                item = jsonArray.ToObject<T>();
            }
            else
            {
                JObject jsonObject = JObject.Parse(json);
                item = jsonObject.ToObject<T>();
            }
            return item;
        }

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
        public async Task<TResponse> PostData<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                int retries = 3; // number of retries 
                int delay = 1000; // initial delay in milliseconds

                while (retries > 0)
                {
                    // Serialize the data object to JSON
                    string json = JsonConvert.SerializeObject(data);

                    // Create a new HttpRequestMessage object with the serialized data
                    var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Send the POST request to the specified endpoint with the serialized data
                    HttpResponseMessage response = await _client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        // Deserialize the response content to the specified type
                        string responseJson = await response.Content.ReadAsStringAsync();
                        TResponse? responseData = JsonConvert.DeserializeObject<TResponse>(responseJson);
                        if (responseData != null)
                        {
                            return responseData;
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        // Wait for a certain amount of time before retrying the request
                        await Task.Delay(delay);

                        // Increase the delay and retry the request
                        delay *= 2;
                        retries--;
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    
                    }
                }
               return default(TResponse)!;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"An HTTP request exception occurred while calling {endpoint}: {ex.Message}");
                throw new HttpRequestException($"Failed to make HTTP request to {endpoint}: {ex.Message}", ex);
            }
        }
    }

}