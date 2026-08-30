using System;
using NPTP.InputSystemWrapper.Enums;
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
        [SerializeField] private string controlSchemeName;
        internal string ControlSchemeName => controlSchemeName;

        [SerializeField] private ControlSchemeBasisSpec basis;
        internal ControlSchemeBasisSpec Basis
        {
            get => basis;
            set => basis = value;
        }

        internal ControlSchemeBasis(string controlSchemeName, ControlSchemeBasisSpec basis)
        {
            this.controlSchemeName = controlSchemeName;
            this.basis = basis;
        }
    }
}
