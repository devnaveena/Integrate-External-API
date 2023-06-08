using IntegrateExternalAPI.Models;
namespace IntegrateExternalAPI.Contracts
{
    public interface IExportToExcel
    {
        string ExportDataToExcel<P, C, T>(User user, List<P> post, List<List<C>> comments, List<T> todos);
    }

}