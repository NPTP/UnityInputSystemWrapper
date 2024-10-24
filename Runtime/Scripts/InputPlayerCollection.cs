using System;
using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using NPTP.InputSystemWrapper.Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace NPTP.InputSystemWrapper
{
    /// <summary>
    /// Useful interface layer for dealing with a collection of multiple players.
    /// Note we avoid foreach & LINQ usage on the internal array to improve performance.
    /// (Our ForEach extension is just a standard array for loop.)
    /// </summary>
    internal sealed class InputPlayerCollection
    {
        private readonly InputPlayer[] players;
        public IEnumerable<InputPlayer> Players => players;
        
        public InputPlayer this[PlayerID id] => players[(int)id];
        public int Count => players.Length;
        
        public InputPlayerCollection(InputActionAsset asset, int size)
        {
            Transform parent = CreateInputParentInScene();
            bool isMultiplayer = size > 1;
            
            players = new InputPlayer[size];
            for (int i = 0; i < size; i++)
            {
                PlayerID id = (PlayerID)i;
                InputPlayer newPlayer = new(asset, id, isMultiplayer, parent);
                players[i] = newPlayer;
            }

            // Loop again as the enabled/disabled handler requires a stable players array,
            // and we're changing the value of player.Enabled here.
            // Player 1 is always enabled by default when a new InputPlayerCollection is created (game is started).
            for (int i = 0; i < players.Length; i++)
            {
                InputPlayer player = players[i];
                player.OnEnabledOrDisabled += HandlePlayerEnabledOrDisabled;
                player.Enabled = player.ID == PlayerID.Player1;
            }
        }

        private Transform CreateInputParentInScene()
        {
            GameObject inputParentGameObject = new() { name = "InputPlayers", transform = { position = Vector3.zero } };
            UnityEngine.Object.DontDestroyOnLoad(inputParentGameObject);
            Transform parent = inputParentGameObject.transform;
            return parent;
        }
        
        public void TerminateAll()
        {
            players.ForEach(p =>
            {
                p.OnEnabledOrDisabled -= HandlePlayerEnabledOrDisabled;
                p.Terminate();
            });
        }

        private void HandlePlayerEnabledOrDisabled(InputPlayer enabledOrDisabledPlayer)
        {
            // If the player is disabled, unpair all their devices to make them available to other players.
            if (!enabledOrDisabledPlayer.Enabled)
            {
                enabledOrDisabledPlayer.UnpairDevices();
            }
            
            int enabledPlayersCount = players.Count(player => player.Enabled);
            if (enabledPlayersCount > 1)
            {
                players.ForEach(p => p.EnableAutoSwitching(false));
            }
            else if (enabledPlayersCount == 1)
            {
                // If there's only one player active, let them switch between all available devices.
                InputPlayer soleEnabledPlayer = players.First(player => player.Enabled);
                soleEnabledPlayer.EnableAutoSwitching(true);
            }
        }

        public bool IsDeviceLastUsedByAnyPlayer(InputDevice device)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].LastUsedDevice == device)
                {
                    return true;
                }
            }

            return false;
        }
        
        public bool AnyPlayerDisabled()
        {
            for (int i = 0; i < players.Length; i++)
            {
                InputPlayer player = players[i];
                if (!player.Enabled) return true;
            }

            return false;
        }
        
        public bool TryGetPlayerPairedWithDevice(InputDevice device, out InputPlayer player)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].IsDevicePaired(device))
                {
                    player = players[i];
                    return true;
                }
            }

            player = null;
            return false;
        }
        
        public bool TryPairDeviceToFirstDisabledPlayer(InputDevice device, out InputPlayer disabledPlayer)
        {
            for (int i = 0; i < players.Length; i++)
            {
                InputPlayer player = players[i];
                if (player.Enabled)
                {
                    continue;
                }
                
                player.PairDevice(device);
                disabledPlayer = player;
                return true;
            }

            disabledPlayer = null;
            return false;
        }

        public void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            for (int i = 0; i < players.Length; i++)
            {
                InputPlayer player = players[i];
                if (player.IsUser(inputUser))
                {
                    player.HandleInputUserChange(inputUserChange, inputDevice);
                    break;
                }
            }
        }

        public void EnableContextForAll(InputContext inputContext)
        {
            players.ForEach(p => p.InputContext = inputContext);
        }

        public void FindActionEventAndSubscribeAll(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool subscribe)
        {
            players.ForEach(p => p.FindActionEventAndSubscribe(actionReference, callback, subscribe));
        }
    }
}