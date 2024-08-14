using System.Collections.Generic;
using NPTP.InputSystemWrapper.Generated.MapActions;
using UnityEngine.InputSystem;

// ------------------------------------------------------------------------
// This file was automatically generated by InputScriptGenerator.
// ------------------------------------------------------------------------
namespace NPTP.InputSystemWrapper.Generated.MapCaches
{
    public class GameplayMapCache
    {
        private readonly List<IGameplayActions> interfaces = new();

        public InputActionMap ActionMap { get; }
        public void Enable() => ActionMap.Enable();
        public void Disable() => ActionMap.Disable();

        public InputAction Thrust { get; }
        public InputAction Shoot { get; }
        public InputAction Hyperspace { get; }
        public InputAction Turn { get; }
        public InputAction Pause { get; }

        public GameplayMapCache(InputActionAsset asset)
        {
            ActionMap = asset.FindActionMap("Gameplay", throwIfNotFound: true);
            
            Thrust = ActionMap.FindAction("Thrust", throwIfNotFound: true);
            Shoot = ActionMap.FindAction("Shoot", throwIfNotFound: true);
            Hyperspace = ActionMap.FindAction("Hyperspace", throwIfNotFound: true);
            Turn = ActionMap.FindAction("Turn", throwIfNotFound: true);
            Pause = ActionMap.FindAction("Pause", throwIfNotFound: true);
        }

        public void AddCallbacks(IGameplayActions instance)
        {
            if (instance == null || interfaces.Contains(instance))
            {
                return;
            }
            
            interfaces.Add(instance);
            
            Thrust.started += instance.OnThrust;
            Thrust.performed += instance.OnThrust;
            Thrust.canceled += instance.OnThrust;
            Shoot.started += instance.OnShoot;
            Shoot.performed += instance.OnShoot;
            Shoot.canceled += instance.OnShoot;
            Hyperspace.started += instance.OnHyperspace;
            Hyperspace.performed += instance.OnHyperspace;
            Hyperspace.canceled += instance.OnHyperspace;
            Turn.started += instance.OnTurn;
            Turn.performed += instance.OnTurn;
            Turn.canceled += instance.OnTurn;
            Pause.started += instance.OnPause;
            Pause.performed += instance.OnPause;
            Pause.canceled += instance.OnPause;
        }

        public void RemoveCallbacks(IGameplayActions instance)
        {
            if (!interfaces.Remove(instance))
            {
                return;
            }

            Thrust.started -= instance.OnThrust;
            Thrust.performed -= instance.OnThrust;
            Thrust.canceled -= instance.OnThrust;
            Shoot.started -= instance.OnShoot;
            Shoot.performed -= instance.OnShoot;
            Shoot.canceled -= instance.OnShoot;
            Hyperspace.started -= instance.OnHyperspace;
            Hyperspace.performed -= instance.OnHyperspace;
            Hyperspace.canceled -= instance.OnHyperspace;
            Turn.started -= instance.OnTurn;
            Turn.performed -= instance.OnTurn;
            Turn.canceled -= instance.OnTurn;
            Pause.started -= instance.OnPause;
            Pause.performed -= instance.OnPause;
            Pause.canceled -= instance.OnPause;
        }
    }
}
