using System;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums
{
    public enum InheritanceModifier
    {
        None = 0,
        Virtual,
        Override,
        Sealed,
        Abstract
    }

    public static class InheritanceModifierExtensions
    {
        public static string AsString(this InheritanceModifier inheritanceModifier)
        {
            return inheritanceModifier switch
            {
                InheritanceModifier.None => string.Empty,
                InheritanceModifier.Virtual => "virtual",
                InheritanceModifier.Override => "override",
                InheritanceModifier.Sealed => "sealed",
                InheritanceModifier.Abstract => "abstract",
                _ => throw new ArgumentOutOfRangeException(nameof(inheritanceModifier), inheritanceModifier, null)
            };
        }
    }
}