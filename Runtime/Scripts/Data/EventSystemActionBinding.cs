using System;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Data
{
    /// <summary>
    /// Associates one event system action slot with the ID of the input action that should fill it.
    /// The ID is resolved against each player's own cloned copy of the input action asset at runtime.
    /// </summary>
    [Serializable]
    internal struct EventSystemActionBinding
    {
        [SerializeField] private EventSystemActionType actionType;
        internal EventSystemActionType ActionType => actionType;

        [SerializeField] private string actionID;
        internal string ActionID => actionID;

#if UNITY_EDITOR
        internal const string EDITOR_ActionTypeField = nameof(actionType);
        internal const string EDITOR_ActionIDField = nameof(actionID);
#endif
    }
}
