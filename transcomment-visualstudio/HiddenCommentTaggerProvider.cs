using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace TransComment.VisualStudio
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(ClassificationTag))]
    internal sealed class HiddenCommentTaggerProvider : ITaggerProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var classificationType = ClassificationRegistry.GetClassificationType(HiddenCommentFormat.ClassificationName);
            return new HiddenCommentTagger(buffer, classificationType) as ITagger<T>;
        }
    }
}
