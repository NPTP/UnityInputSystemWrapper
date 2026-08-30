using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Authoring-time record of which device family a control scheme is based on. Keyed by the control
    /// scheme's name in the input action asset rather than the generated enum, so that the package does
    /// not depend on generated code.
    /// </summary>
    [Serializable]
    internal class ControlSchemeBasis
    {
        internal enum BasisSpec
        {
            Undefined = 0,
            IsMouseBased,
            IsGamepadBased
        }

        [SerializeField] private string controlSchemeName;
        internal string ControlSchemeName => controlSchemeName;

        [SerializeField] private BasisSpec basis;
        internal BasisSpec Basis
        {
            get => basis;
            set => basis = value;
        }

        internal ControlSchemeBasis(string controlSchemeName, BasisSpec basis)
        {
            this.controlSchemeName = controlSchemeName;
            this.basis = basis;
        }
    }
}
