using IntegrateExternalAPI.Models;
namespace IntegrateExternalAPI.Repository
{
    public interface IExternalApiService
    {

        /// <summary>
        /// The function "ExternalService" returns a string.
        /// </summary>
         Task<string> ExternalService();

    }
}