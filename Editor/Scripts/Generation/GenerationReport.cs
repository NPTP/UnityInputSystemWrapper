using System.Collections.Generic;
using NPTP.UnitySourceGen.Editor.ScriptWriting;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Collects everything a generation run touched so it can be reported as one console entry rather than
    /// one per file. Errors are still logged as they happen, since they need their own console entry.
    /// </summary>
    internal static class GenerationReport
    {
        private static List<string> entries;

        internal static void Begin()
        {
            entries = new List<string>();
            writtenPaths.Clear();
            unchangedCount = 0;
        }

        /// <summary>
        /// Discard the report without logging it, for runs that aborted before doing any real work.
        /// </summary>
        internal static void End()
        {
            entries = null;
        }

        /// <summary>
        /// Record what a write did. Files already up to date are counted rather than listed, so the report
        /// shows what actually changed.
        /// </summary>
        internal static void RecordWrite(string assetPath, ScriptWriteResult result)
        {
            writtenPaths.Add(assetPath);

            switch (result)
            {
                case ScriptWriteResult.Written:
                    Record(assetPath);
                    break;
                case ScriptWriteResult.Unchanged:
                    unchangedCount++;
                    break;
            }
        }

        private static int unchangedCount;

        private static readonly HashSet<string> writtenPaths = new();

        /// <summary>True if this run produced the file, whether it changed or was already correct.</summary>
        internal static bool WasWritten(string assetPath) => writtenPaths.Contains(assetPath);

        internal static void Record(string entry)
        {
            entries?.Add(entry);
        }

        internal static void LogAndEnd(string header)
        {
            if (entries == null)
            {
                return;
            }

            if (entries.Count == 0)
            {
                ISWDebug.Log($"{header}: everything already up to date ({unchangedCount} files).");
                entries = null;
                unchangedCount = 0;
                return;
            }

            ISWDebug.Log($"{header} ({entries.Count}):\n  {string.Join("\n  ", entries)}");
            entries = null;
        }
    }
}
