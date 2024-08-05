using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityInputSystemWrapper.Generated.MapActions;

// ------------------------------------------------------------------------
// This file was automatically generated by InputScriptGenerator.
// ------------------------------------------------------------------------
namespace UnityInputSystemWrapper.Generated.MapCaches
{
    public class PauseMenuMapCache
    {
        private readonly List<IPauseMenuActions> interfaces = new();

        public InputActionMap ActionMap { get; }
        public void Enable() => ActionMap.Enable();
        public void Disable() => ActionMap.Disable();

        public InputAction Navigate { get; }
        public InputAction Submit { get; }
        public InputAction Unpause { get; }

        public PauseMenuMapCache(InputActionAsset asset)
        {
            ActionMap = asset.FindActionMap("PauseMenu", throwIfNotFound: true);
            
            Navigate = ActionMap.FindAction("Navigate", throwIfNotFound: true);
            Submit = ActionMap.FindAction("Submit", throwIfNotFound: true);
            Unpause = ActionMap.FindAction("Unpause", throwIfNotFound: true);
        }

        public void AddCallbacks(IPauseMenuActions instance)
        {
            if (instance == null || interfaces.Contains(instance))
            {
                return;
            }
            
            interfaces.Add(instance);
            
            Navigate.started += instance.OnNavigate;
            Navigate.performed += instance.OnNavigate;
            Navigate.canceled += instance.OnNavigate;
            Submit.started += instance.OnSubmit;
            Submit.performed += instance.OnSubmit;
            Submit.canceled += instance.OnSubmit;
            Unpause.started += instance.OnUnpause;
            Unpause.performed += instance.OnUnpause;
            Unpause.canceled += instance.OnUnpause;
        }

        public void RemoveCallbacks(IPauseMenuActions instance)
        {
            if (!interfaces.Remove(instance))
            {
                return;
            }

            Navigate.started -= instance.OnNavigate;
            Navigate.performed -= instance.OnNavigate;
            Navigate.canceled -= instance.OnNavigate;
            Submit.started -= instance.OnSubmit;
            Submit.performed -= instance.OnSubmit;
            Submit.canceled -= instance.OnSubmit;
            Unpause.started -= instance.OnUnpause;
            Unpause.performed -= instance.OnUnpause;
            Unpause.canceled -= instance.OnUnpause;
        }
    }
}
