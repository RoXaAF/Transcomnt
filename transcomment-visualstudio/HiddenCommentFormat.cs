using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace TransComment.VisualStudio
{
    [Export(typeof(ClassificationTypeDefinition))]
    [Name(ClassificationName)]
    internal static class HiddenCommentClassification
    {
        public const string ClassificationName = "transcomment.hiddenComment";
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationName)]
    [Name(ClassificationName)]
    [UserVisible(false)]
    [Order(Before = Priority.Default)]
    internal sealed class HiddenCommentFormat : ClassificationFormatDefinition
    {
        public const string ClassificationName = HiddenCommentClassification.ClassificationName;

        public HiddenCommentFormat()
        {
            DisplayName = "TransComment Hidden Comment";
            ForegroundColor = Colors.Transparent;
        }
    }
}
