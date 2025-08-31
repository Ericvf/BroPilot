using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;

namespace BroPilot
{
    public class VisualStudioContextProvider : IContextProvider
    {
        public async Task<string> GetActiveDocument()
        {
            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView?.TextView != null)
            {
                var snapshot = docView.TextView.TextBuffer.CurrentSnapshot;
                return snapshot.GetText(); // This returns the actual text
            }

            return "No active document";
        }


        //   private readonly DTE devToolsEnvironment;

        //public VisualStudioContextProvider(EnvDTE.DTE devToolsEnvironment)
        //{
        //    this.devToolsEnvironment = devToolsEnvironment;
        //}

        public string GetSolutionName()
        {
            //      return devToolsEnvironment?.Solution?.FullName ?? "Unknown";
            var dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            return dte?.Solution?.FullName ?? "Unknown";
        }
    }
}
