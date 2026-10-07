using System;

namespace TransComment.VisualStudio
{
    internal static class TransCommentState
    {
        private static bool hidden;

        public static event EventHandler Changed;

        public static bool Hidden
        {
            get => hidden;
            set
            {
                if (hidden == value)
                {
                    return;
                }

                hidden = value;
                Changed?.Invoke(null, EventArgs.Empty);
            }
        }
    }
}
