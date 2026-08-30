using System;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Enums
{
    /// <summary>
    /// Identifies one control scheme without naming the generated ControlScheme enum, so that the package
    /// runtime does not depend on generated code. Carries a copy of the scheme's baked metadata, so
    /// resolving a name or a basis never needs a lookup back into the runtime input data.
    /// </summary>
    internal readonly struct ControlSchemeId : IEquatable<ControlSchemeId>
    {
        internal const int NONE_INDEX = -1;

        /// <summary>
        /// Index into the control scheme list on the input action asset, matching the generated enum's value.
        /// </summary>
        internal int Index { get; }

        /// <summary>
        /// The control scheme's name as it appears in the input action asset, for use with the Input System API.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// Which device families the scheme uses.
        /// </summary>
        internal ControlSchemeBasisSpec Basis { get; }

        internal bool UsesPointer => Has(ControlSchemeBasisSpec.UsesPointer);
        internal bool UsesGamepad => Has(ControlSchemeBasisSpec.UsesGamepad);
        internal bool UsesKeyboard => Has(ControlSchemeBasisSpec.UsesKeyboard);
        internal bool UsesJoystick => Has(ControlSchemeBasisSpec.UsesJoystick);
        internal bool UsesSensor => Has(ControlSchemeBasisSpec.UsesSensor);
        internal bool UsesTrackedDevice => Has(ControlSchemeBasisSpec.UsesTrackedDevice);

        private bool Has(ControlSchemeBasisSpec family) => (Basis & family) != 0;

        internal bool IsNone => Index == NONE_INDEX;

        internal static ControlSchemeId None => new(NONE_INDEX, string.Empty, ControlSchemeBasisSpec.Undefined);

        internal ControlSchemeId(int index, string name, ControlSchemeBasisSpec basis)
        {
            Index = index;
            Name = name;
            Basis = basis;
        }

        internal InputBinding ToBindingMask() => new(groups: Name, path: default);

        public bool Equals(ControlSchemeId other) => Index == other.Index;
        public override bool Equals(object obj) => obj is ControlSchemeId other && Equals(other);
        public override int GetHashCode() => Index;
        public override string ToString() => IsNone ? "None" : Name;

        public static bool operator ==(ControlSchemeId a, ControlSchemeId b) => a.Equals(b);
        public static bool operator !=(ControlSchemeId a, ControlSchemeId b) => !a.Equals(b);
    }
}
