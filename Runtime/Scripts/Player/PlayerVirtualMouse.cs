using System.Collections.Generic;
using NPTP.InputSystemWrapper.Components;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

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

        /// <summary>The player's other mice, set aside while the virtual one drives their UI.</summary>
        private readonly List<InputDevice> unpairedMice = new();

        /// <summary>The device being driven, or null while this is off.</summary>
        internal Mouse Device => virtualMouseInput == null ? null : virtualMouseInput.virtualMouse;

        internal bool Enabled => gameObject != null;

        internal PlayerVirtualMouse(InputPlayer player, InputData inputData)
        {
            this.player = player;
            this.inputData = inputData;
        }

        /// <summary>
        /// Start driving a mouse from the player's virtual mouse actions. Does nothing when the map named
        /// on the input data is missing or does not hold what a virtual mouse map must.
        /// </summary>
        /// <param name="cursorParent">Where the cursor is put, or null to leave it at the scene's root.</param>
        internal void Enable(RectTransform cursorParent)
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
            SetUpCursor(cursorParent);

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
                UnpairOtherMice();
            }
        }

        /// <summary>Stop driving the mouse, take its device away and put its cursor away.</summary>
        internal void Disable()
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

            RepairOtherMice();

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
        /// Put this player's own copy of the cursor on screen. Left at the scene's root unless a parent is
        /// given, since a cursor draws through a canvas of its own.
        /// </summary>
        private void SetUpCursor(RectTransform cursorParent)
        {
            GameObject prefab = inputData.VirtualMouseCursorPrefab;
            if (prefab == null)
            {
                return;
            }

            // Checked on the prefab rather than on a copy of it, so a cursor that could not work is never
            // built. What it names is what the copy's own component names, so the copy needs no checking.
            ISWVirtualMouseUI prefabCursorUI = prefab.GetComponent<ISWVirtualMouseUI>();
            if (prefabCursorUI == null)
            {
                ISWDebug.LogWarning($"The virtual mouse cursor prefab \"{prefab.name}\" has no " +
                                    $"{nameof(ISWVirtualMouseUI)} on its root, so no cursor is shown.");
                return;
            }

            if (prefabCursorUI.CursorTransform == null || prefabCursorUI.CursorGraphic == null)
            {
                ISWDebug.LogWarning($"The {nameof(ISWVirtualMouseUI)} on \"{prefab.name}\" needs both a cursor " +
                                    "transform and a cursor graphic, so no cursor is shown.");
                return;
            }

            cursor = cursorParent == null ? Object.Instantiate(prefab) : Object.Instantiate(prefab, cursorParent);
            cursor.name = $"Player[{player.ID.ToString()}]VirtualMouseCursor";

            // The graphic decides which canvas the cursor is held inside the bounds of, and is the one the
            // mouse hides when the hardware cursor draws instead.
            ISWVirtualMouseUI cursorUI = cursor.GetComponent<ISWVirtualMouseUI>();

            // Started in the middle of the screen rather than in the corner its anchors put it, since the
            // mouse takes the cursor's position as its own when it starts driving it.
            cursorUI.CursorTransform.anchoredPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            virtualMouseInput.cursorGraphic = cursorUI.CursorGraphic;
            virtualMouseInput.cursorTransform = cursorUI.CursorTransform;
        }

        /// <summary>
        /// Take the player's other mice off them for as long as the virtual one is driving their UI.
        /// Left as the player's only mouse, the action binds to a single control and reads it live,
        /// which is what makes its actions land - otherwise it conflicts with the other mice.
        /// </summary>
        private void UnpairOtherMice()
        {
            foreach (InputDevice pairedDevice in player.User.pairedDevices)
            {
                if (pairedDevice is Mouse && pairedDevice != Device)
                {
                    unpairedMice.Add(pairedDevice);
                }
            }

            foreach (InputDevice mouse in unpairedMice)
            {
                player.User.UnpairDevice(mouse);
            }
        }

        /// <summary>Give the player back the mice taken from them, if they are still around.</summary>
        private void RepairOtherMice()
        {
            if (player.User.valid)
            {
                foreach (InputDevice mouse in unpairedMice)
                {
                    if (mouse.added)
                    {
                        InputUser.PerformPairingWithDevice(mouse, player.User);
                    }
                }
            }

            unpairedMice.Clear();
        }

        private static InputActionProperty PropertyFor(InputActionMap actionMap, string actionName)
        {
            InputAction action = actionMap.FindAction(actionName, throwIfNotFound: false);
            return action == null ? default : new InputActionProperty(action);
        }
    }
}
