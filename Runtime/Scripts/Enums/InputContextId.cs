using System;

namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// Identifies one input context without naming the generated InputContext enum, so that the package
    /// runtime does not depend on generated code.
    /// </summary>
    internal readonly struct InputContextId : IEquatable<InputContextId>
    {
        /// <summary>
        /// Index into the input context list on the runtime input data, matching the generated enum's value.
        /// </summary>
        internal int Index { get; }

        internal InputContextId(int index)
        {
            Index = index;
        }

        public bool Equals(InputContextId other) => Index == other.Index;
        public override bool Equals(object obj) => obj is InputContextId other && Equals(other);
        public override int GetHashCode() => Index;
        public override string ToString() => Index.ToString();

        public static bool operator ==(InputContextId a, InputContextId b) => a.Equals(b);
        public static bool operator !=(InputContextId a, InputContextId b) => !a.Equals(b);
    }
}
