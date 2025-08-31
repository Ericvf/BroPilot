using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using System.Linq;
using System.Threading.Tasks;

namespace BroPilot
{
    public class VisualStudioContextProvider : IContextProvider
    {
        private DTE dte;

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

        public async Task<string> GetCurrentMethod()
        {
            // Get the active document
            DocumentView docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView?.TextView == null)
                return "No active document";

            // Get the text buffer
            ITextSnapshot snapshot = docView.TextView.TextBuffer.CurrentSnapshot;
            string sourceText = snapshot.GetText();

            // Get caret position
            int caretPosition = docView.TextView.Caret.Position.BufferPosition.Position;

            // Parse into syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceText);
            var root = await syntaxTree.GetRootAsync();

            // Find the token at the caret
            var token = root.FindToken(caretPosition);

            // Walk up the tree to find the enclosing method
            var methodDeclaration = token.Parent?.AncestorsAndSelf()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            if (methodDeclaration != null)
            {
                // Full string representation of the method
                string methodText = methodDeclaration.ToFullString();

                // Method name
                string methodName = methodDeclaration.Identifier.Text;

                return $"Method Name: {methodName}\n\n{methodText}";
            }

            return "Caret not inside a method";
        }

        public string GetSolutionName()
        {
            DTE dte = GetDevelopmentEnvironment();
            return dte?.Solution?.FullName ?? "Unknown";
        }

        private DTE GetDevelopmentEnvironment()
        {
            if (dte == null)
            {

                ThreadHelper.ThrowIfNotOnUIThread();
                dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
            }

            return dte;
        }
    }
}
