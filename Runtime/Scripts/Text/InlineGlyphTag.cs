using NPTP.InputSystemWrapper.Enums;

namespace NPTP.InputSystemWrapper.Text
{
    /// <summary>
    /// One "&lt;isw ...&gt;" element found in a string: which binding it names, how it should be shown, and
    /// where it sits in the string it was read from so it can be replaced in place.
    /// </summary>
    public readonly struct InlineGlyphTag
    {
        /// <summary>Where the element starts in the source string.</summary>
        public int StartIndex { get; }

        /// <summary>How many characters of the source string the element occupies.</summary>
        public int Length { get; }

        /// <summary>Whether to show the binding's sprite or its display name.</summary>
        public InlineGlyphType GlyphType { get; }

        /// <summary>Whose bindings to read.</summary>
        public int PlayerID { get; }

        /// <summary>The action map named for full specificity, or empty when the action name stands alone.</summary>
        public string ActionMapName { get; }

        public string ActionName { get; }

        /// <summary>Which part of a composite to show, or DontIsolatePart for the binding as a whole.</summary>
        public CompositePart CompositePart { get; }

        /// <summary>Which of the action's bindings on the current control scheme to show.</summary>
        public int UIIndex { get; }

        internal InlineGlyphTag(int startIndex, int length, InlineGlyphType glyphType, int playerID,
            string actionMapName, string actionName, CompositePart compositePart, int uiIndex)
        {
            StartIndex = startIndex;
            Length = length;
            GlyphType = glyphType;
            PlayerID = playerID;
            ActionMapName = actionMapName;
            ActionName = actionName;
            CompositePart = compositePart;
            UIIndex = uiIndex;
        }

        public override string ToString()
        {
            string action = string.IsNullOrEmpty(ActionMapName) ? ActionName : $"{ActionMapName}.{ActionName}";
            return $"<isw type={GlyphType} player={PlayerID} action=\"{action}\" composite={CompositePart} index={UIIndex}>";
        }
    }
}
