using System;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Data
{
    [Serializable]
    public class EventSystemActionSpecification
    {
        [SerializeField] private EventSystemActionType actionType;
        public EventSystemActionType ActionType => actionType;
        
        [SerializeField] private InputActionReference actionReference;
        public InputActionReference ActionReference => actionReference;
    }
}