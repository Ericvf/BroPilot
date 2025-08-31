using System.Threading.Tasks;

namespace BroPilot
{
    public interface IContextProvider
    {
        string GetSolutionName();

        Task<string> GetActiveDocument();

        Task<string> GetCurrentMethod();
    }
}
