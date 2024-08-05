using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace UnityInputSystemWrapper
{
    public class InputPlayerCollection : IEnumerable<InputPlayer>
    {
        private readonly SortedList<int, InputPlayer> players = new();

        public InputPlayer CreateAndAddPlayer(InputActionAsset asset, InputDevice pairWithDevice = null)
        {
            int id = GetSmallestUnusedID();
            InputPlayer newPlayer = new(asset, id);
            
            if (pairWithDevice != null)
            {
                // NP TODO: What to do with the device?
            }
            
            players.Add(id, newPlayer);
            return newPlayer;
        }

        public void EnableContextForAll(InputContext inputContext) => ForEach(p => p.EnableContext(inputContext));
        public void TerminateAll() => ForEach(p => p.Terminate());
        public void FindActionEventAndSubscribeAll(InputActionReference actionReference, Action<InputAction.CallbackContext> callback, bool subscribe)
            => ForEach(p => p.FindActionEventAndSubscribe(actionReference, callback, subscribe));

        private void ForEach(Action<InputPlayer> action)
        {
            foreach (InputPlayer player in players.Values)
                action?.Invoke(player);
        }

        public bool TryGetPlayer(int id, out InputPlayer player)
        {
            return players.TryGetValue(id, out player);
        }

        private int GetSmallestUnusedID()
        {
            int smallestID = -1;
            foreach (int id in players.Keys)
            {
                if (smallestID < id - 1)
                    break;

                smallestID = id;
            }

            return smallestID + 1;
        }

        public bool Contains(InputPlayer player) => players.ContainsKey(player.ID);
        public bool Remove(InputPlayer player) => players.Remove(player.ID);
        public void Clear() => players.Clear();
        
        public IEnumerator<InputPlayer> GetEnumerator() => players.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
