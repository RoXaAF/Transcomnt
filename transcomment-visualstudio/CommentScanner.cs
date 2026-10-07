using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace TransComment.VisualStudio
{
    internal static class CommentScanner
    {
        private static readonly (string Start, string End)[] BlockTokens =
        {
            ("/*", "*/"),
            ("<!--", "-->"),
            ("<#", "#>"),
            ("\"\"\"", "\"\"\""),
            ("'''", "'''")
        };

        private static readonly string[] LineTokens = { "//", "#", "--" };

        public static IEnumerable<SnapshotSpan> FindCommentSpans(ITextSnapshot snapshot)
        {
            string text = snapshot.GetText();

            foreach (var token in BlockTokens)
            {
                foreach (var span in FindBlockSpans(snapshot, text, token.Start, token.End))
                {
                    yield return span;
                }
            }

            foreach (var span in FindLineSpans(snapshot))
            {
                yield return span;
            }
        }

        private static IEnumerable<SnapshotSpan> FindBlockSpans(ITextSnapshot snapshot, string text, string startToken, string endToken)
        {
            int searchAt = 0;
            while (searchAt < text.Length)
            {
                int start = text.IndexOf(startToken, searchAt, StringComparison.Ordinal);
                if (start < 0)
                {
                    yield break;
                }

                int bodyStart = start + startToken.Length;
                int end = text.IndexOf(endToken, bodyStart, StringComparison.Ordinal);
                if (end < 0)
                {
                    yield break;
                }

                int length = end + endToken.Length - start;
                yield return new SnapshotSpan(snapshot, start, length);
                searchAt = end + endToken.Length;
            }
        }

        private static IEnumerable<SnapshotSpan> FindLineSpans(ITextSnapshot snapshot)
        {
            for (int i = 0; i < snapshot.LineCount; i++)
            {
                ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);
                string lineText = line.GetText();
                int commentStart = FindFirstLineComment(lineText);
                if (commentStart >= 0)
                {
                    yield return new SnapshotSpan(snapshot, line.Start.Position + commentStart, lineText.Length - commentStart);
                }
            }
        }

        private static int FindFirstLineComment(string line)
        {
            int best = -1;
            foreach (string token in LineTokens)
            {
                int found = FindLineCommentStart(line, token);
                if (found >= 0 && (best < 0 || found < best))
                {
                    best = found;
                }
            }

            return best;
        }

        private static int FindLineCommentStart(string text, string token)
        {
            bool inSingle = false;
            bool inDouble = false;
            bool inBacktick = false;

            for (int i = 0; i <= text.Length - token.Length; i++)
            {
                char current = text[i];
                char previous = i > 0 ? text[i - 1] : '\0';
                bool escaped = previous == '\\';

                if (!escaped)
                {
                    if (current == '\'' && !inDouble && !inBacktick)
                    {
                        inSingle = !inSingle;
                    }
                    else if (current == '"' && !inSingle && !inBacktick)
                    {
                        inDouble = !inDouble;
                    }
                    else if (current == '`' && !inSingle && !inDouble)
                    {
                        inBacktick = !inBacktick;
                    }
                }

                if (!inSingle && !inDouble && !inBacktick && StartsWithAt(text, token, i))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool StartsWithAt(string text, string token, int index)
        {
            if (index + token.Length > text.Length)
            {
                return false;
            }

            for (int i = 0; i < token.Length; i++)
            {
                if (text[index + i] != token[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
