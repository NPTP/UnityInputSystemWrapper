using System.Collections.Generic;

namespace NPTP.InputSystemWrapper.Editor
{
    /// <summary>
    /// Info used for code generation around MARKERs left in the code to signal spots to change or generate things.
    /// </summary>
    internal class InputScriptGeneratorMarkerInfo
    {
        public string MarkerName { get; }
        public string LeadingWhiteSpace { get; }
        public List<string> NewLines { get; }
        
        public InputScriptGeneratorMarkerInfo(string markerName, string leadingWhiteSpace, List<string> newLines)
        {
            MarkerName = markerName;
            LeadingWhiteSpace = leadingWhiteSpace;
            NewLines = newLines;
        }
    }
}