using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace TransComment.VisualStudio
{
    internal sealed class HiddenCommentTagger : ITagger<ClassificationTag>, IDisposable
    {
        private readonly ITextBuffer buffer;
        private readonly ClassificationTag tag;

        public HiddenCommentTagger(ITextBuffer buffer, IClassificationType classificationType)
        {
            this.buffer = buffer;
            tag = new ClassificationTag(classificationType);
            TransCommentState.Changed += OnHiddenStateChanged;
            buffer.Changed += OnBufferChanged;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (!TransCommentState.Hidden || spans.Count == 0)
            {
                yield break;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;
            foreach (var span in CommentScanner.FindCommentSpans(snapshot))
            {
                if (spans.IntersectsWith(new NormalizedSnapshotSpanCollection(span)))
                {
                    yield return new TagSpan<ClassificationTag>(span, tag);
                }
            }
        }

        public void Dispose()
        {
            TransCommentState.Changed -= OnHiddenStateChanged;
            buffer.Changed -= OnBufferChanged;
        }

        private void OnHiddenStateChanged(object sender, EventArgs e)
        {
            RaiseChanged(buffer.CurrentSnapshot);
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (TransCommentState.Hidden)
            {
                RaiseChanged(e.After);
            }
        }

        private void RaiseChanged(ITextSnapshot snapshot)
        {
            var span = new SnapshotSpan(snapshot, 0, snapshot.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }
    }
}
