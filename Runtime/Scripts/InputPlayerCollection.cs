using System;
using System.Collections;
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
    /// </summary>
    internal sealed class InputPlayerCollection : IEnumerable<InputPlayer>
    {
        internal event Action<InputPlayer> OnPlayerAdded;
        internal event Action<int> OnPlayerRemoved;
        
        private readonly InputActionAsset inputActionAsset;
        private readonly Transform inputParent;
        private InputPlayer[] players = Array.Empty<InputPlayer>();
        
        private IEnumerable<InputPlayer> Players => players.Where(player => player != null);
        private int PlayerCount => Players.Count();
        
        public IEnumerator<InputPlayer> GetEnumerator() => Players.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        internal InputPlayer this[int playerID]
        {
            get
            {
                Add(playerID);
                return players[playerID];
            }
        }
        
        internal InputPlayerCollection(InputActionAsset asset)
        {
            inputParent = CreateInputParentInScene();
            inputActionAsset = asset;
        }

        #region Internal Methods
        
        internal void Add(int playerID)
        {
            if (playerID >= players.Length)
            {
                InputPlayer[] extended = new InputPlayer[playerID - 1];
                Array.Copy(players, extended, players.Length);
                players = extended;
            }

            if (players[playerID] != null)
            {
                return;
            }

            InputPlayer newPlayer = new InputPlayer(inputActionAsset, playerID, true, inputParent);
            players[playerID] = newPlayer;
            newPlayer.OnEnabledOrDisabled += HandlePlayerEnabledOrDisabled;
            newPlayer.Enabled = true;
#if UNITY_EDITOR
            newPlayer.EDITOR_OnInputContextChanged += EDITOR_HandlePlayerInputContextChanged;
#endif
            
            foreach (InputPlayer player in Players)
            {
                player.IsMultiplayer = true;
            }
            
            OnPlayerAdded?.Invoke(newPlayer);
        }

        internal void Remove(int playerID)
        {
            if (playerID <= 0)
            {
                Debug.LogError("Cannot terminate the default player.");
                return;
            }
            
            if (playerID >= players.Length || players[playerID] == null)
            {
                return;
            }

            players[playerID].Terminate();
            players[playerID] = null;
            
            if (PlayerCount == 1)
            {
                players[0].IsMultiplayer = false;
            }
            
            OnPlayerRemoved?.Invoke(playerID);
        }
        
        internal void TerminateAll()
        {
            foreach (InputPlayer player in players)
            {
                player.OnEnabledOrDisabled -= HandlePlayerEnabledOrDisabled;
                player.Terminate();
#if UNITY_EDITOR
                player.EDITOR_OnInputContextChanged -= EDITOR_HandlePlayerInputContextChanged;
#endif
            }

            players.DefaultAll();
        }

        internal bool IsDeviceLastUsedByAnyPlayer(InputDevice device)
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
        
        internal bool AnyPlayerDisabled()
        {
            for (int i = 0; i < players.Length; i++)
            {
                InputPlayer player = players[i];
                if (!player.Enabled) return true;
            }

            return false;
        }
        
        internal bool TryGetPlayerPairedWithDevice(InputDevice device, out InputPlayer player)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    continue;
                }
                
                if (players[i].IsDevicePaired(device))
                {
                    player = players[i];
                    return true;
                }
            }

            player = null;
            return false;
        }

        // TODO (optimization): ActionWrapper should have a playerID perhaps, or link to player, or something, to optimize this.
        internal bool TryGetPlayerAssociatedWithAsset(InputActionAsset asset, out InputPlayer playerAssociatedWithAsset)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    continue;
                }

                InputPlayer player = players[i];
                if (player.Asset == asset)
                {
                    playerAssociatedWithAsset = player;
                    return true;
                }
            }

            playerAssociatedWithAsset = null;
            return false;
        }
        
        internal bool TryPairDeviceToFirstDisabledPlayer(InputDevice device, out InputPlayer pairedPlayer)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    continue;
                }

                InputPlayer player = players[i];
                if (player.Enabled)
                {
                    continue;
                }
                
                player.PairDevice(device);
                pairedPlayer = player;
                return true;
            }

            pairedPlayer = null;
            return false;
        }

        internal void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
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

        internal void SetContextForAll(InputContext inputContext)
        {
            foreach (InputPlayer player in players)
            {
                player.InputContext = inputContext;
            }
        }
        
        #endregion

        #region Private Methods

        private Transform CreateInputParentInScene()
        {
            GameObject inputParentGameObject = new() { name = "InputPlayers", transform = { position = Vector3.zero } };
            UnityEngine.Object.DontDestroyOnLoad(inputParentGameObject);
            Transform parent = inputParentGameObject.transform;
            return parent;
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
                foreach (InputPlayer player in players)
                {
                    player.EnableAutoSwitching(false);
                }
            }
            else if (enabledPlayersCount == 1)
            {
                // If there's only one player active, let them switch between all available devices.
                InputPlayer soleEnabledPlayer = players.First(player => player.Enabled);
                soleEnabledPlayer.EnableAutoSwitching(true);
            }
        }
        
        #endregion
        
        #region Editor-Only Debug Fields/Properties/Methods
#if UNITY_EDITOR
        internal event Action<InputPlayer> EDITOR_OnPlayerInputContextChanged;

        private void EDITOR_HandlePlayerInputContextChanged(InputPlayer inputPlayer)
        {
            EDITOR_OnPlayerInputContextChanged?.Invoke(inputPlayer);
        }
#endif
        #endregion
    }
}