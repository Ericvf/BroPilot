using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace BroPilot.Extension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(ToolWindow1))]
    [ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]

    public sealed class BroPilotExtensionPackage : AsyncPackage
    {
        public const string PackageGuidString = "76c1045f-0c12-49b4-88bc-984d6375f401";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name).Name + ".dll";
                var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // VSIX folder
                var path = Path.Combine(folder, name);
                if (File.Exists(path))
                    return Assembly.LoadFrom(path);
                return null;
            };

            await Task.WhenAll(
                ToolWindow1Command.InitializeAsync(this),
                Command1.InitializeAsync(this));
        }
    }
}
