using System;
using System.Collections.Generic;
using System.Text;

namespace TransComment.VisualStudio
{
    internal static class CommentTransformer
    {
        private const string Marker = "transcomment:";

        public static string TransformAll(string text, string language)
        {
            var rules = GetRules(language);
            var result = new StringBuilder(text.Length + 128);
            bool inSingle = false;
            bool inDouble = false;
            bool inBacktick = false;

            for (int i = 0; i < text.Length;)
            {
                var block = MatchBlockToken(text, i, rules.BlockTokens);
                if (!inSingle && !inDouble && !inBacktick && block.Start != null)
                {
                    int bodyStart = i + block.Start.Length;
                    int end = text.IndexOf(block.End, bodyStart, StringComparison.Ordinal);
                    int bodyEnd = end >= 0 ? end : text.Length;

                    result.Append(block.Start);
                    AppendMarkerIfMissing(result, text, bodyStart);
                    result.Append(text, bodyStart, bodyEnd - bodyStart);

                    if (end >= 0)
                    {
                        result.Append(block.End);
                        i = end + block.End.Length;
                    }
                    else
                    {
                        i = text.Length;
                    }

                    continue;
                }

                string lineToken = MatchLineToken(text, i, rules.LineTokens);
                if (!inSingle && !inDouble && !inBacktick && lineToken != null)
                {
                    int bodyStart = i + lineToken.Length;
                    int lineEnd = FindLineEnd(text, bodyStart);

                    result.Append(lineToken);
                    AppendMarkerIfMissing(result, text, bodyStart);
                    result.Append(text, bodyStart, lineEnd - bodyStart);
                    i = lineEnd;
                    continue;
                }

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

                if (current == '\r' || current == '\n')
                {
                    inSingle = false;
                    inDouble = false;
                    inBacktick = false;
                }

                result.Append(current);
                i++;
            }

            return result.ToString();
        }

        private static CommentRules GetRules(string language)
        {
            string normalized = (language ?? string.Empty).ToLowerInvariant();

            switch (normalized)
            {
                case "python":
                case "yaml":
                case "powershell":
                    return new CommentRules(new[] { "#" }, Array.Empty<(string Start, string End)>());
                case "sql":
                    return new CommentRules(new[] { "--" }, new[] { ("/*", "*/") });
                case "html":
                case "xml":
                    return new CommentRules(Array.Empty<string>(), new[] { ("<!--", "-->") });
                case "css":
                    return new CommentRules(Array.Empty<string>(), new[] { ("/*", "*/") });
                default:
                    return new CommentRules(new[] { "//" }, new[] { ("/*", "*/") });
            }
        }

        private static (string Start, string End) MatchBlockToken(
            string text,
            int index,
            IReadOnlyList<(string Start, string End)> tokens)
        {
            foreach (var token in tokens)
            {
                if (StartsWithAt(text, token.Start, index))
                {
                    return token;
                }
            }

            return (null, null);
        }

        private static string MatchLineToken(string text, int index, IReadOnlyList<string> tokens)
        {
            foreach (string token in tokens)
            {
                if (StartsWithAt(text, token, index))
                {
                    return token;
                }
            }

            return null;
        }

        private static int FindLineEnd(string text, int index)
        {
            while (index < text.Length && text[index] != '\r' && text[index] != '\n')
            {
                index++;
            }

            return index;
        }

        private static void AppendMarkerIfMissing(StringBuilder result, string text, int bodyStart)
        {
            int firstContent = bodyStart;
            while (firstContent < text.Length && char.IsWhiteSpace(text[firstContent]) && text[firstContent] != '\r' && text[firstContent] != '\n')
            {
                firstContent++;
            }

            if (StartsWithAt(text, Marker, firstContent, ignoreCase: true))
            {
                return;
            }

            result.Append(' ');
            result.Append(Marker);
            result.Append(' ');
        }

        private static bool StartsWithAt(string text, string token, int index, bool ignoreCase = false)
        {
            if (token == null || index < 0 || index + token.Length > text.Length)
            {
                return false;
            }

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Compare(text, index, token, 0, token.Length, comparison) == 0;
        }

        private sealed class CommentRules
        {
            public CommentRules(IReadOnlyList<string> lineTokens, IReadOnlyList<(string Start, string End)> blockTokens)
            {
                LineTokens = lineTokens;
                BlockTokens = blockTokens;
            }

            public IReadOnlyList<string> LineTokens { get; }

            public IReadOnlyList<(string Start, string End)> BlockTokens { get; }
        }
    }
}
