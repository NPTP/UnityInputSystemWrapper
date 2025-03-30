using System;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    [Serializable]
    public class ControlSchemeBasis
    {
        internal enum BasisSpec
        {
            Undefined = 0,
            IsMouseBased,
            IsGamepadBased
        }
        
        [SerializeField] private ControlScheme controlScheme;
        internal ControlScheme ControlScheme => controlScheme;

        [SerializeField] private BasisSpec basis;
        internal BasisSpec Basis
        {
            get => basis;
            set => basis = value;
        }
        
        internal ControlSchemeBasis(ControlScheme controlScheme, BasisSpec basis)
        {
            this.controlScheme = controlScheme;
            this.basis = basis;
        }
    }
}