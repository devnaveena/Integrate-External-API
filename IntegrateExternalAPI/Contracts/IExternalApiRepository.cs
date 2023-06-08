using IntegrateExternalAPI.Models;
namespace IntegrateExternalAPI.Repository
{
    public interface IExternalApiRepository
    {

        /// <summary>
        /// This function retrieves a list of objects of type T from a specified endpoint.
        /// </summary>
        /// <param name="endPoint">The endPoint parameter is a string that represents the endpoint of a web
        /// API that the method is trying to accessand</param>
        Task<List<T>> GetAsync<T>(string endPoint);


        /// <summary>
        /// This is a generic function that retrieves an object of type T by its ID from a specified
        /// endpoint.
        /// </summary>
        /// <param name="Id">The Id parameter is a long integer value that represents the unique
        /// identifier of the object that you want to retrieve from the specified endpoint.</param>
        /// <param name="endPoint">The endPoint parameter is a string that represents the endpoint or URL
        /// of the API that you want to call to retrieve the data for the specified Id</param>
        Task<T> GetByIdAsync<T>(long Id, string endPoint);

        /// <summary>
        /// This function retrieves a list of objects of type T by their ID from a specified endpoint and
        /// resource.
        /// </summary>
        /// <param name="Id">The Id parameter is of type long and represents the unique identifier of the
        /// resource that we want to retrieve.</param>
        /// <param name="endPoint">The endpoint is a string that represents the URL endpoint of the API
        /// that you want to call."/</param>
        /// <param name="resource">The "resource" parameter is likely a string that represents the specific
        /// resource or entity that the method is trying to retrieve from the API.</param>
         Task<List<T>> GetByIdAsync<T>(long Id, string endPoint, string resource);


        /// <summary>
        /// This is a generic function that sends a POST request to a specified endpoint with provided
        /// data and resource.
        /// </summary>
        /// <param name="T">T is a generic type parameter that represents the type of data being posted
        /// to the specified endpoint and resource. </param>
        /// <param name="Id">The Id parameter is of type long and is used to identify a specific resource
        /// in the API endpoint</param>
        /// <param name="endPoint">The endPoint parameter in the Post method is a string that represents
        /// the URL endpoint where the HTTP POST request will be sent</param>
        /// <param name="resource">The "resource" parameter in the given method is a string that
        /// represents the specific resource or endpoint that the data is being posted to</param>
         Task<T> PostAsync<T>(T data, long Id, string endPoint, string resource);


    }
}