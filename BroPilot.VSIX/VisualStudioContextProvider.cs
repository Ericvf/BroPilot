using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
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

        public string GetCurrentFileName()
        {
            DTE dte = GetDevelopmentEnvironment();
            var fullName = dte?.ActiveDocument?.FullName;
            var solutionPath = dte?.Solution?.FullName;

            if (string.IsNullOrEmpty(fullName))
                return "Unknown";

            if (!string.IsNullOrEmpty(solutionPath))
            {
                var solutionDir = System.IO.Path.GetDirectoryName(solutionPath);
                if (solutionDir != null && fullName.StartsWith(solutionDir))
                {
                    return fullName.Substring(solutionDir.Length + 1).Replace('/', '\\');
                }
            }

            return System.IO.Path.GetFileName(fullName);
        }

        public string GetSolutionName()
        {
            DTE dte = GetDevelopmentEnvironment();
            return dte?.Solution?.FullName ?? "Unknown";
        }

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

        private static Microsoft.CodeAnalysis.Document GetDocumentFromWorkspace(string file, Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace workspace)
        {
            return workspace.CurrentSolution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => ConvertFolderSeparator(d.FilePath).EndsWith(ConvertFolderSeparator(file)));
        }

        private Microsoft.CodeAnalysis.Document GetActiveDocumentFromWorkspace(Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace workspace)
        {
            var docName = GetCurrentFileName();
            var doc = workspace.CurrentSolution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => d.FilePath.EndsWith(docName));
            return doc;
        }

        private async Task ApplyNewRootAsync(Microsoft.CodeAnalysis.Document doc, SyntaxNode newRoot)
        {
            var formattedRoot = Formatter.Format(newRoot, doc.Project.Solution.Workspace);
            var newDoc = doc.WithSyntaxRoot(formattedRoot);
            var newText = await newDoc.GetTextAsync();

            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            if (docView?.TextView == null)
                return;

            using (var edit = docView.TextBuffer.CreateEdit())
            {
                edit.Replace(0, docView.TextBuffer.CurrentSnapshot.Length, newText.ToString());
                edit.Apply();
            }

            await VS.Commands.ExecuteAsync("Edit.FormatDocument");
        }

        public async Task AddClass(string file, string classname, string code)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //var docView = await VS.Documents.GetActiveDocumentViewAsync();
            //var componentModel = await VS.Services.GetComponentModelAsync();
            //var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();
            //var doc = GetActiveDocumentFromWorkspace(workspace);

            var (docView, doc, workspace) = await GetDocumentAndWorkspace();

            if (docView?.TextView == null)
                return;

            if (doc == null)
                return;

            var root = await doc.GetSyntaxRootAsync();

            var rootNamespace = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

            var newClass = SyntaxFactory.ParseMemberDeclaration(code) as ClassDeclarationSyntax;
            if (newClass == null)
                return;

            SyntaxNode newRoot;

            if (rootNamespace != null)
            {
                var newNamespace = rootNamespace.AddMembers(newClass);
                newRoot = root.ReplaceNode(rootNamespace, newNamespace);
            }
            else
            {
                var compilationUnit = (CompilationUnitSyntax)root;
                newRoot = compilationUnit.AddMembers(newClass);
            }

            var formattedRoot = Formatter.Format(newRoot, workspace);

            var newDoc = doc.WithSyntaxRoot(formattedRoot);
            var newText = await newDoc.GetTextAsync();

            using (var edit = docView.TextBuffer.CreateEdit())
            {
                edit.Replace(0, docView.TextBuffer.CurrentSnapshot.Length, newText.ToString());
                edit.Apply();
            }

            await VS.Commands.ExecuteAsync("Edit.FormatDocument");
        }

        public async Task AddMethod(string file, string classname, string code)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //var docView = await VS.Documents.GetActiveDocumentViewAsync();
            //if (docView?.TextView == null)
            //    return;

            //var componentModel = await VS.Services.GetComponentModelAsync();
            //var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

            ////var doc = GetDocumentFromWorkspace(file, workspace);
            //var doc = GetActiveDocumentFromWorkspace(workspace);
            //if (doc == null)
            //    return;
            var (docView, doc, workspace) = await GetDocumentAndWorkspace();

            if (docView?.TextView == null)
                return;

            if (doc == null)
                return;

            var root = await doc.GetSyntaxRootAsync();

            var sourceClass = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == classname);

            if (sourceClass == null)
                return;

            var newMethod = SyntaxFactory.ParseMemberDeclaration(code) as MethodDeclarationSyntax;
            if (newMethod == null)
                return;

            var newClass = sourceClass.AddMembers(newMethod);
            var newRoot = root.ReplaceNode(sourceClass, newClass);
            
            var formattedRoot = Formatter.Format(newRoot, workspace);

            var newDoc = doc.WithSyntaxRoot(formattedRoot);
            var newText = await newDoc.GetTextAsync();

            using (var edit = docView.TextBuffer.CreateEdit())
            {
                edit.Replace(0, docView.TextBuffer.CurrentSnapshot.Length, newText.ToString());
                edit.Apply();
            }

            await VS.Commands.ExecuteAsync("Edit.FormatDocument");
        }
        
        public async Task ReplaceClass(string file, string classname, string code)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //var docView = await VS.Documents.GetActiveDocumentViewAsync();
            //if (docView?.TextView == null)
            //    return;

            //var componentModel = await VS.Services.GetComponentModelAsync();
            //var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

            //var doc = GetActiveDocumentFromWorkspace(workspace);
            //if (doc == null)
            //    return;

            var (docView, doc, workspace) = await GetDocumentAndWorkspace();

            if (docView?.TextView == null)
                return;

            if (doc == null)
                return;

            var root = await doc.GetSyntaxRootAsync();

            var sourceClass = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == classname);

            if (sourceClass == null)
                return;

            if (code.Contains("\\n"))
                code = code.Replace("\\n", "\n");

            var replacementClass = SyntaxFactory.ParseMemberDeclaration(code) as ClassDeclarationSyntax;
            if (replacementClass == null)
                return;

            var newRoot = root.ReplaceNode(sourceClass, replacementClass);
            var formattedRoot = Formatter.Format(newRoot, workspace);

            var newDoc = doc.WithSyntaxRoot(formattedRoot);
            var newText = await newDoc.GetTextAsync();

            using (var edit = docView.TextBuffer.CreateEdit())
            {
                edit.Replace(0, docView.TextBuffer.CurrentSnapshot.Length, newText.ToString());
                edit.Apply();
            }

            await VS.Commands.ExecuteAsync("Edit.FormatDocument");
        }

        public async Task ReplaceMethod(string file, string classname, string method, string code)
        {
            // Switch to main thread, required for VS services
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            //var componentModel = await VS.Services.GetComponentModelAsync();
            //var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();

            //var docView = await VS.Documents.GetActiveDocumentViewAsync();
            //if (docView?.TextView == null)
            //    return;

            //var doc = GetActiveDocumentFromWorkspace(workspace);
            //if (doc == null)
            //    return;
            var (docView, doc, workspace) = await GetDocumentAndWorkspace();

            if (docView?.TextView == null)
                return;

            if (doc == null)
                return;

            var root = await doc.GetSyntaxRootAsync();

            var sourceMethod = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == method);

            if (sourceMethod != null)
            {
                var replacementMethod = SyntaxFactory.ParseMemberDeclaration(code);

                if (replacementMethod == null)
                    return;

                var newRoot = root.ReplaceNode(sourceMethod, replacementMethod);

                var formattedRoot = Formatter.Format(newRoot, workspace);

                var newDoc = doc.WithSyntaxRoot(formattedRoot);
                var newText = await newDoc.GetTextAsync();

                using (var edit = docView.TextBuffer.CreateEdit())
                {
                    edit.Replace(0, docView.TextBuffer.CurrentSnapshot.Length, newText.ToString());
                    edit.Apply();
                }
            }

            if (sourceMethod == null)
            {
                var replacementMethod = SyntaxFactory.ParseStatement(code);
                if (replacementMethod == null)
                    return;

                var localFunctionStatementSyntax =
                        root.DescendantNodes()
                              .OfType<LocalFunctionStatementSyntax>()
                              .FirstOrDefault(m => m.Identifier.Text == method);

                if (localFunctionStatementSyntax != null)
                {
                    var newRoot = root.ReplaceNode(localFunctionStatementSyntax, replacementMethod);

                    var formattedRoot = Formatter.Format(newRoot, workspace);

                    var newDoc = doc.WithSyntaxRoot(formattedRoot);
                    var newText = await newDoc.GetTextAsync();

                    using (var edit = docView.TextBuffer.CreateEdit())
                    {
                        edit.Replace(0, docView.TextBuffer.CurrentSnapshot.Length, newText.ToString());
                        edit.Apply();
                    }
                }
            }

            await VS.Commands.ExecuteAsync("Edit.FormatDocument");
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

        private static string ConvertFolderSeparator(string input) => input.Replace('/', '\\');

        private async Task<(DocumentView, Microsoft.CodeAnalysis.Document, Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace)> GetDocumentAndWorkspace()
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var componentModel = await VS.Services.GetComponentModelAsync();
            var workspace = componentModel.GetService<Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace>();
            var doc = GetActiveDocumentFromWorkspace(workspace);
            return (docView, doc, workspace);
        }

        //public async Task ReplaceMethod(string file, string classname, string method, string code)
        //{
        //    // Switch to main thread, required for VS services
        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        //    var docView = await VS.Documents.GetActiveDocumentViewAsync();
        //    if (docView?.TextView == null)
        //        return;

        //    var snapshot = docView.TextBuffer.CurrentSnapshot;
        //    var text = snapshot.GetText();

        //    // Regex die een hele methode vangt: signature + accolades
        //    // Dit is vrij simplistisch: matched "static int GetMatrixValue(...){...}"
        //    var pattern = $@"static[\s\S]+?\b{method}\b\s*\([^)]*\)\s*\{{[\s\S]*?\}}";

        //    var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
        //    if (!match.Success)
        //        return;

        //    using (var edit = docView.TextBuffer.CreateEdit())
        //    {
        //        edit.Replace(match.Index, match.Length, string.Empty);
        //        edit.Apply();
        //    }


        //    await VS.Commands.ExecuteAsync("Edit.FormatDocument");
        //}
    }
}
