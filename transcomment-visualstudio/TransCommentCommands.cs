using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace TransComment.VisualStudio
{
    internal sealed class TransCommentCommands
    {
        private readonly AsyncPackage package;

        private TransCommentCommands(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package;

            commandService.AddCommand(new MenuCommand(
                ToggleComments,
                new CommandID(new Guid(Guids.CommandSetGuidString), Guids.ToggleCommentsCommandId)));

            commandService.AddCommand(new MenuCommand(
                AddComment,
                new CommandID(new Guid(Guids.CommandSetGuidString), Guids.AddCommentCommandId)));

            commandService.AddCommand(new MenuCommand(
                TransformAllComments,
                new CommandID(new Guid(Guids.CommandSetGuidString), Guids.TransformAllCommentsCommandId)));
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                _ = new TransCommentCommands(package, commandService);
            }
        }

        private void ToggleComments(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            TransCommentState.Hidden = !TransCommentState.Hidden;

            VsShellUtilities.ShowMessageBox(
                package,
                TransCommentState.Hidden ? "TransComment comments are hidden." : "TransComment comments are visible.",
                "TransComment",
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private async void AddComment(object sender, EventArgs e)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            var selection = dte?.ActiveDocument?.Selection as TextSelection;
            if (selection == null)
            {
                return;
            }

            string prefix = GetLineCommentPrefix(dte.ActiveDocument?.Language);
            if (selection.IsEmpty)
            {
                selection.Text = $"{prefix} transcomment: ";
                return;
            }

            selection.Text = $"{prefix} transcomment: {selection.Text}";
        }

        private async void TransformAllComments(object sender, EventArgs e)
        {
            await package.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = await package.GetServiceAsync(typeof(DTE)) as DTE2;
            var document = dte?.ActiveDocument;
            var textDocument = document?.Object("TextDocument") as TextDocument;
            if (textDocument == null)
            {
                return;
            }

            EditPoint start = textDocument.StartPoint.CreateEditPoint();
            EditPoint end = textDocument.EndPoint.CreateEditPoint();
            string original = start.GetText(end);
            string transformed = CommentTransformer.TransformAll(original, document.Language);

            if (transformed == original)
            {
                VsShellUtilities.ShowMessageBox(
                    package,
                    "No comments needed a TransComment marker.",
                    "TransComment",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return;
            }

            start.ReplaceText(end, transformed, (int)vsEPReplaceTextOptions.vsEPReplaceTextKeepMarkers);
        }

        private static string GetLineCommentPrefix(string language)
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                return "//";
            }

            switch (language.ToLowerInvariant())
            {
                case "python":
                case "yaml":
                case "powershell":
                    return "#";
                case "sql":
                    return "--";
                case "html":
                case "xml":
                    return "<!--";
                default:
                    return "//";
            }
        }
    }
}
