using System.Collections.Generic;
using NPTP.InputSystemWrapper.Components;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

namespace NPTP.InputSystemWrapper.Player
{
    /// <summary>
    /// A mouse one player drives with the actions of the virtual mouse map, so a gamepad can move a cursor.
    /// The device is paired to that player, so it feeds their UI and nobody else's.
    /// </summary>
    internal sealed class PlayerVirtualMouse
    {
        private readonly InputPlayer player;
        private readonly InputData inputData;

        private GameObject gameObject;
        private GameObject cursor;
        private VirtualMouseInput virtualMouseInput;

        /// <summary>The device being driven, or null while this is off.</summary>
        internal Mouse Device => virtualMouseInput == null ? null : virtualMouseInput.virtualMouse;

        internal bool Enabled
        {
            get => gameObject != null;
            set
            {
                if (value)
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }
        }

        internal PlayerVirtualMouse(InputPlayer player, InputData inputData)
        {
            this.player = player;
            this.inputData = inputData;
        }

        /// <summary>
        /// Start driving a mouse from the player's virtual mouse actions. Does nothing when the map named
        /// on the input data is missing or does not hold what a virtual mouse map must.
        /// </summary>
        private void Enable()
        {
            if (Enabled)
            {
                return;
            }

            string mapName = inputData.VirtualMouseActionMapName;
            InputActionMap actionMap = string.IsNullOrEmpty(mapName)
                ? null
                : player.Asset.FindActionMap(mapName, throwIfNotFound: false);

            List<string> problems = VirtualMouseMapSpec.Problems(actionMap);
            if (problems.Count > 0)
            {
                ISWDebug.LogWarning($"Player {player.ID.ToString()} cannot drive a virtual mouse from the " +
                                    $"\"{mapName}\" map: {string.Join(" ", problems)}");
                return;
            }

            // Built inactive so its actions and cursor are in place before it adds its device and starts
            // reading them, which it does the moment it is enabled.
            gameObject = new GameObject($"Player[{player.ID.ToString()}]VirtualMouse");
            gameObject.transform.SetParent(player.PlayerInputTransform, worldPositionStays: false);
            gameObject.SetActive(false);

            virtualMouseInput = gameObject.AddComponent<VirtualMouseInput>();
            virtualMouseInput.cursorMode = inputData.VirtualMouseCursorMode;
            SetUpCursor();

            virtualMouseInput.stickAction = PropertyFor(actionMap, VirtualMouseMapSpec.MOVE);
            virtualMouseInput.leftButtonAction = PropertyFor(actionMap, VirtualMouseMapSpec.LEFT_BUTTON);
            virtualMouseInput.rightButtonAction = PropertyFor(actionMap, VirtualMouseMapSpec.RIGHT_BUTTON);
            virtualMouseInput.middleButtonAction = PropertyFor(actionMap, VirtualMouseMapSpec.MIDDLE_BUTTON);
            virtualMouseInput.scrollWheelAction = PropertyFor(actionMap, VirtualMouseMapSpec.SCROLL_WHEEL);

            gameObject.SetActive(true);

            // Paired so its input reaches this player's actions and counts as theirs rather than as a stray
            // device, which is what would otherwise move them onto a control scheme wanting a mouse.
            if (Device != null && player.User.valid)
            {
                InputUser.PerformPairingWithDevice(Device, player.User);
            }
        }

        /// <summary>Stop driving the mouse, take its device away and put its cursor away.</summary>
        private void Disable()
        {
            if (!Enabled)
            {
                return;
            }

            Mouse device = Device;
            if (device != null && player.User.valid)
            {
                player.User.UnpairDevice(device);
            }

            // Destroying the object disables the component, which removes the device and gives the system
            // mouse back if it had taken it.
            Object.Destroy(gameObject);
            gameObject = null;
            virtualMouseInput = null;

            if (cursor != null)
            {
                Object.Destroy(cursor);
                cursor = null;
            }
        }

        /// <summary>
        /// Put this player's own copy of the cursor on screen. A cursor lives at the root of the scene
        /// rather than under the player, since it draws through a canvas of its own.
        /// </summary>
        private void SetUpCursor()
        {
            ISWVirtualMouseUI prefab = inputData.VirtualMouseCursorPrefab;
            if (prefab == null)
            {
                return;
            }

            ISWVirtualMouseUI instance = Object.Instantiate(prefab);
            cursor = instance.gameObject;
            cursor.name = $"Player[{player.ID.ToString()}]VirtualMouseCursor";

            Graphic cursorGraphic = instance.CursorGraphic;
            if (cursorGraphic == null)
            {
                ISWDebug.LogWarning($"The virtual mouse cursor prefab \"{prefab.name}\" names no cursor graphic, " +
                                    "so nothing in it can be moved with the mouse.");
                return;
            }

            // What moves is the graphic's own transform, not the cursor's root: a cursor carries its own
            // canvas, and a canvas drives its rect transform itself, overwriting anything moving it. The
            // graphic also decides which canvas the cursor is held inside the bounds of.
            virtualMouseInput.cursorGraphic = cursorGraphic;
            virtualMouseInput.cursorTransform = cursorGraphic.rectTransform;
        }

        private static InputActionProperty PropertyFor(InputActionMap actionMap, string actionName)
        {
            InputAction action = actionMap.FindAction(actionName, throwIfNotFound: false);
            return action == null ? default : new InputActionProperty(action);
        }
    }
}
