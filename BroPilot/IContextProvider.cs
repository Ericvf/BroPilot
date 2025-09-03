using System.Threading.Tasks;

namespace BroPilot
{
    public interface IContextProvider
    {
        string GetSolutionName();

        Task<string> GetActiveDocument();

        Task<string> GetCurrentMethod();

        Task ReplaceMethod(string file, string classname, string method, string code);

        Task ReplaceClass(string file, string classname, string code);

        Task AddMethod(string file, string classname, string code);

        Task AddClass(string file, string classname, string code);

        string GetCurrentFileName();
    }
}
