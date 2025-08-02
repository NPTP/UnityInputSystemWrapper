using System.Collections.Generic;

namespace NPTP.InputSystemWrapper.Editor
{
    /// <summary>
    /// Info used for code generation around MARKERs left in the code to signal spots to change or generate things.
    /// </summary>
    internal class InputScriptGeneratorMarkerInfo
    {
        internal string MarkerName { get; }
        internal List<string> NewLines { get; }
        
        internal InputScriptGeneratorMarkerInfo(string markerName, string leadingWhiteSpace, List<string> newLines)
        {
            MarkerName = markerName;
            NewLines = newLines;
        }
    }
}