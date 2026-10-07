using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;

namespace TransComment.VisualStudio
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("TransComment", "Hide/show comments and add quick TransComment notes.", "0.1.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.PackageGuidString)]
    public sealed class TransCommentPackage : AsyncPackage
    {
        protected override async System.Threading.Tasks.Task InitializeAsync(
            CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await TransCommentCommands.InitializeAsync(this);
        }
    }
}
