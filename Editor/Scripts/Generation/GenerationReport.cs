using System.Collections.Generic;
using NPTP.InputSystemWrapper.Editor.Utilities;
using UnityEngine;

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
        }

        /// <summary>
        /// Discard the report without logging it, for runs that aborted before doing any real work.
        /// </summary>
        internal static void End()
        {
            entries = null;
        }

        internal static void RecordFile(string systemFilePath)
        {
            entries?.Add(ToProjectRelativePath(systemFilePath));
        }

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
                entries = null;
                return;
            }

            ISWDebug.Log($"{header} ({entries.Count}):\n  {string.Join("\n  ", entries)}");
            entries = null;
        }

        private static string ToProjectRelativePath(string systemFilePath)
        {
            string path = systemFilePath.Replace('\\', '/');
            string assetsPath = Application.dataPath;
            return path.StartsWith(assetsPath) ? "Assets" + path.Substring(assetsPath.Length) : path;
        }
    }
}
