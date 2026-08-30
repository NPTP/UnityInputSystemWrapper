using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums;
using NPTP.InputSystemWrapper.Utilities.Extensions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace NPTP.InputSystemWrapper.Player
{
    /// <summary>
    /// Useful interface layer for dealing with a collection of multiple players.
    /// </summary>
    internal sealed class InputPlayerCollection : IEnumerable<InputPlayer>
    {
        private const int DEFAULT_PLAYER_PLAYER_ID = 0;

        internal InputPlayer DefaultPlayer { get; }

        private IEnumerable<InputPlayer> Players => players.Where(player => player != null);

        private readonly RuntimeInputData runtimeInputData;
        private readonly Transform inputParent;
        private Action<InputPlayer> onPlayerAdded;
        private Action<int> onPlayerRemoved;
        private InputPlayer[] players = Array.Empty<InputPlayer>();

        internal InputPlayerCollection(RuntimeInputData runtimeInputData, Action<InputPlayer> playerAddedListener, Action<int> playerRemovedListener)
        {
            this.runtimeInputData = runtimeInputData;
            inputParent = CreateInputParentInScene();
            
            // Add default player before setting player added listener,
            // since this object is not created yet and external listeners may try to access it.
            DefaultPlayer = GetOrAdd(DEFAULT_PLAYER_PLAYER_ID);
            
            onPlayerAdded = playerAddedListener;
            onPlayerRemoved = playerRemovedListener;
        }
        
        public IEnumerator<InputPlayer> GetEnumerator() => Players.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Internal Methods

        internal InputPlayer GetOrAdd(int playerID)
        {
            if (playerID >= players.Length)
            {
                InputPlayer[] extended = new InputPlayer[playerID + 1];
                Array.Copy(players, extended, players.Length);
                players = extended;
            }
            else if (players[playerID] != null)
            {
                return players[playerID];
            }

            InputPlayer newPlayer = new InputPlayer(runtimeInputData, playerID, true, inputParent);
            players[playerID] = newPlayer;
            newPlayer.OnEnabledOrDisabled += HandlePlayerEnabledOrDisabled;
            newPlayer.Enabled = true;
#if UNITY_EDITOR
            newPlayer.EDITOR_OnInputContextChanged += EDITOR_HandlePlayerInputContextChanged;
#endif
            
            onPlayerAdded?.Invoke(newPlayer);
            return newPlayer;
        }

        internal void Remove(int playerID)
        {
            if (playerID <= DEFAULT_PLAYER_PLAYER_ID)
            {
                Debug.LogError($"Cannot remove the default player or get a player with ID < {DEFAULT_PLAYER_PLAYER_ID}.");
                return;
            }
            
            if (playerID >= players.Length || players[playerID] == null)
            {
                return;
            }

            players[playerID].Terminate();
            players[playerID] = null;

            onPlayerRemoved?.Invoke(playerID);
        }
        
        internal void Terminate()
        {
            foreach (InputPlayer player in Players)
            {
                player.OnEnabledOrDisabled -= HandlePlayerEnabledOrDisabled;
                player.Terminate();
#if UNITY_EDITOR
                player.EDITOR_OnInputContextChanged -= EDITOR_HandlePlayerInputContextChanged;
#endif
            }

            onPlayerAdded = null;
            onPlayerRemoved = null;
        }
        
        public void SetMultiplayer(bool isMultiplayer)
        {
            foreach (InputPlayer player in Players)
            {
                player.IsMultiplayer = isMultiplayer;
            }
        }

        internal bool IsDeviceLastUsedByAnyPlayer(InputDevice device)
        {
            return Players.Any(player => player.LastUsedDevice == device);
        }
        
        internal bool AnyPlayerDisabled()
        {
            return Players.Any(player => !player.Enabled);
        }

        internal bool TryGetPlayer(int playerID, out InputPlayer player)
        {
            if (!players.IndexIsValid(playerID) || players[playerID] == null)
            {
                player = default;
                return false;
            }

            player = players[playerID];
            return true;
        }
        
        internal bool TryGetPlayerPairedWithDevice(InputDevice device, out InputPlayer pairedPlayer)
        {
            foreach (var player in Players)
            {
                if (player.IsDevicePaired(device))
                {
                    pairedPlayer = player;
                    return true;
                }
            }

            pairedPlayer = null;
            return false;
        }

        internal bool TryPairDeviceToFirstDisabledPlayer(InputDevice device, out InputPlayer pairedPlayer)
        {
            foreach (var player in Players)
            {
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

        internal void PairDeviceToNewPlayer(InputDevice device)
        {
            AddFirstPossiblePlayerID().PairDevice(device);
        }

        internal void HandleInputUserChange(InputUser inputUser, InputUserChange inputUserChange, InputDevice inputDevice)
        {
            foreach (InputPlayer player in Players)
            {
                if (player.IsUser(inputUser))
                {
                    player.HandleInputUserChange(inputUserChange, inputDevice);
                    break;
                }
            }
        }

        internal void SetContextForAll(InputContext inputContext)
        {
            foreach (InputPlayer player in Players)
            {
                player.InputContext = inputContext;
            }
        }
        
        #endregion

        #region Private Methods
        
        /// <summary>
        /// Add a new player at the first possible player ID.
        /// This may be between, or greater than any existing player IDs.
        /// </summary>
        private InputPlayer AddFirstPossiblePlayerID()
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == null)
                {
                    return GetOrAdd(i);
                }
            }

            return GetOrAdd(players.Length);
        }

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

            int enabledPlayersCount = Players.Count(player => player.Enabled);
            
            if (enabledPlayersCount > 1)
            {
                foreach (InputPlayer player in Players)
                {
                    player.IsMultiplayer = true;
                }
            }
            else if (enabledPlayersCount == 1)
            {
                InputPlayer soleEnabledPlayer = Players.First(player => player.Enabled);
                soleEnabledPlayer.IsMultiplayer = false;
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