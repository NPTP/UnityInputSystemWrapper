using System;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Data
{
    [Serializable]
    internal class EventSystemActionSpecification
    {
        [SerializeField] private EventSystemActionType actionType;
        internal EventSystemActionType ActionType => actionType;
        
        [SerializeField] private InputActionReference actionReference;
        internal InputActionReference ActionReference => actionReference;
    }
}