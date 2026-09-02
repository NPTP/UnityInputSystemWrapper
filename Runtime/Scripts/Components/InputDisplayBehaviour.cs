using System;
using NPTP.InputSystemWrapper.Player;
using UnityEngine;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// Base for anything that shows bindings and has to load them to do it. Holds what was loaded, gives it
    /// back on disable or reload, and loads again whenever the bindings or the device in use change.
    /// </summary>
    /// <typeparam name="T">What one load produces, which owns the assets it was built from.</typeparam>
    public abstract class InputDisplayBehaviour<T> : MonoBehaviour where T : class, IDisposable
    {
        /// <summary>What is on screen now, or null when nothing has been loaded yet.</summary>
        protected T Loaded { get; private set; }

        /// <summary>Tells a load that is no longer wanted to release its result instead of showing it.</summary>
        private int loadGeneration;

        /// <summary>
        /// Whether there is anything to load. A display with no action chosen keeps what it has.
        /// </summary>
        protected virtual bool CanLoad => true;

        protected virtual void OnEnable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange += HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged += HandleBindingsChanged;
            Refresh();
        }

        protected virtual void OnDisable()
        {
            InputRuntime.Current.OnAnyPlayerInputUserChange -= HandleAnyPlayerInputUserChange;
            InputRuntime.Current.OnBindingsChanged -= HandleBindingsChanged;

            loadGeneration++;
            Release();
        }

        /// <summary>Load and show this again, e.g. after changing in code what is shown.</summary>
        public void Refresh()
        {
            if (!CanLoad)
            {
                return;
            }

            int generation = ++loadGeneration;
            Load(loaded =>
            {
                if (generation != loadGeneration)
                {
                    loaded?.Dispose();
                    return;
                }

                Release();
                Loaded = loaded;
                Display(loaded);
            });
        }

        /// <summary>Start one load, calling back with its result once it is ready.</summary>
        protected abstract void Load(Action<T> onLoaded);

        /// <summary>Put a loaded result on screen. Called again without loading when only what to show changes.</summary>
        protected abstract void Display(T loaded);

        /// <summary>Anything a subclass built alongside the loaded result and has to give back with it.</summary>
        protected virtual void OnReleased()
        {
        }

        /// <summary>Show what is already loaded again, for a change that needs no new assets.</summary>
        protected void Redisplay()
        {
            if (Loaded != null)
            {
                Display(Loaded);
            }
        }

        private void Release()
        {
            Loaded?.Dispose();
            Loaded = null;
            OnReleased();
        }

        private void HandleAnyPlayerInputUserChange(InputUserChangeInfo inputUserChangeInfo) => Refresh();
        private void HandleBindingsChanged() => Refresh();
    }
}
