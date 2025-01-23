using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor
{
    /// <summary>
    /// Info used for code generation around MARKERs left in the code to signal spots to change or generate things.
    /// </summary>
    internal class InputScriptGeneratorMarkerInfo
    {
        public InputActionAsset InputActionAsset { get; }
        public string MarkerName { get; }
        public string LeadingWhiteSpace { get; }
        public List<string> NewLines { get; }
        
        public InputScriptGeneratorMarkerInfo(InputActionAsset inputActionAsset, string markerName, string leadingWhiteSpace, List<string> newLines)
        {
            InputActionAsset = inputActionAsset;
            MarkerName = markerName;
            LeadingWhiteSpace = leadingWhiteSpace;
            NewLines = newLines;
        }
    }
}