using System;
using UnityEngine;

namespace NPTP.InputSystemWrapper.CustomSetups
{
    public abstract class CustomSetup : ScriptableObject
    {
        protected abstract Type SetupType { get; }
        protected virtual string Name => string.Empty;
        internal abstract void Register();
    }
}
