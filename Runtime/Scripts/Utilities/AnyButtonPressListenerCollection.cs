using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.AnyButtonPress;
using NPTP.InputSystemWrapper.Player;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NPTP.InputSystemWrapper.Utilities
{
    internal class AnyButtonPressListenerCollection
    {
        internal int Count => listeners.Count;
            
        private readonly HashSet<AnyButtonPressListener> listeners = new();
        private IDisposable anyButtonPressCaller;

        protected virtual void HandleAnyButtonPressed(InputControl inputControl)
        {
            // Temp arrays for invocation instead of enumerating the stored collections, since
            // listeners could unsubscribe during invocation which would modify those collections.

            foreach (AnyButtonPressListener listener in listeners.ToArray())
            {
                listener?.Invoke(inputControl);
            }
        }

        internal void Clear()
        {
            listeners.Clear();
            DisposeAnyButtonPressCallerIfNoListeners();
        }

        private void PopulateAnyButtonPressCaller()
        {
            anyButtonPressCaller ??= InputSystem.onAnyButtonPress.Call(HandleAnyButtonPressed);
        }
            
        private void DisposeAnyButtonPressCallerIfNoListeners()
        {
            if (listeners.Count > 0 || anyButtonPressCaller == null)
            {
                return;
            }
            
            anyButtonPressCaller.Dispose();
            anyButtonPressCaller = null;
        }

        internal bool Add(AnyButtonPressListener listener)
        {
            if (listener == null)
            {
                return false;
            }
                
            if (listeners.Add(listener))
            {
                PopulateAnyButtonPressCaller();
                return true;
            }

            return false;
        }

        internal bool Remove(AnyButtonPressListener listener)
        {
            if (listeners.Remove(listener))
            {
                DisposeAnyButtonPressCallerIfNoListeners();
                return true;
            }

            return false;
        }
    }

    internal class SpecificPlayerAnyButtonPressListenerCollection : AnyButtonPressListenerCollection
    {
        private readonly InputPlayer inputPlayer;
            
        public SpecificPlayerAnyButtonPressListenerCollection(InputPlayer inputPlayer)
        {
            this.inputPlayer = inputPlayer;
        }

        protected override void HandleAnyButtonPressed(InputControl inputControl)
        {
            if (inputPlayer == null || !inputPlayer.IsDevicePaired(inputControl.device))
            {
                return;
            }
                
            base.HandleAnyButtonPressed(inputControl);
        }
    }
}