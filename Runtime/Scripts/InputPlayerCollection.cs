using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

namespace UnityInputSystemWrapper
{
    public class InputPlayerCollection
    {
        private readonly InputPlayer[] players;
        private Transform inputPlayersParent;
        
        public InputPlayer this[PlayerID id] => players[(int)id];

        public InputPlayerCollection(InputActionAsset asset, int size)
        {
            players = new InputPlayer[size];
            for (int i = 0; i < size; i++)
            {
                PlayerID id = (PlayerID)i;
                InputPlayer newPlayer = new(asset, id, null);
                players[i] = newPlayer;
            }

            ++InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleControlsChanged;
        }

        public void TerminateAll()
        {
            ForEach(p => p.Terminate());

            --InputUser.listenForUnpairedDeviceActivity;
            InputUser.onChange += HandleControlsChanged;
        }

        public bool IsDeviceCurrentlyUsed(InputDevice device)
        {
            for (int i = 0; i < players.Length; i++)
            {
                ReadOnlyArray<InputDevice> devices = players[i].PairedDevices;
                for (int j = 0; j < devices.Count; j++)
                {
                    if (device == devices[j])
                        return true;
                }
            }

            return false;
        }

        private void HandleControlsChanged(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            if (inputUserChange != InputUserChange.ControlsChanged)
            {
                return;
            }

            ForEach(p => { if (p.IsUser(inputUser)) p.ProcessControlsChange(inputDevice); });
        }

        public void EnableContextForAll(InputContext inputContext) => ForEach(p => p.CurrentContext = inputContext);

        public void FindActionEventAndSubscribeAll(InputActionReference actionReference,
            Action<InputAction.CallbackContext> callback, bool subscribe) =>
            ForEach(p => p.FindActionEventAndSubscribe(actionReference, callback, subscribe));

        private void ForEach(Action<InputPlayer> action)
        {
            for (int i = 0; i < players.Length; i++)
            {
                action?.Invoke(players[i]);
            }
        }
        
        public void SetUpSceneObjects()
        {
            GameObject inputParentGameObject = new() { name = "InputPlayers", transform = { position = Vector3.zero } };
            UnityEngine.Object.DontDestroyOnLoad(inputParentGameObject);
            inputPlayersParent = inputParentGameObject.transform;
            
            foreach (InputPlayer player in players)
            {
                GameObject playerInputGameObject = new GameObject
                {
                    name = $"{player.ID}Input",
                    transform = { position = Vector3.zero, parent = inputPlayersParent }
                };

                bool isMultiplayer = players.Length > 1;
                
                PlayerInput playerInput = playerInputGameObject.AddComponent<PlayerInput>();
                playerInput.neverAutoSwitchControlSchemes = isMultiplayer;
                
                if (isMultiplayer)
                    playerInputGameObject.AddComponent<MultiplayerEventSystem>();
                else
                    playerInputGameObject.AddComponent<EventSystem>();
                
                InputSystemUIInputModule uiInputModule = playerInputGameObject.AddComponent<InputSystemUIInputModule>();

                player.SetPlayerInputAndUIInputModule(playerInputGameObject, uiInputModule, playerInput);
            }
        }
    }
}