using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.AnyButtonPress
{
    /// <summary>
    /// Custom yield instruction for coroutines to make waiting for any button press a lot more syntactically convenient.
    /// To listen for ANY player:
    /// yield return new WaitForAnyButtonPress();
    /// To listen to a specific player:
    /// yield return new WaitForAnyButtonPress(int playerID);
    /// </summary>
    public class WaitForAnyButtonPress : CustomYieldInstruction
    {
        public override bool keepWaiting
        {
            get
            {
                if (anyButtonPressed || !ISW.DoesPlayerExist(playerID))
                {
                    ResetYieldInstruction();
                    return false;
                }

                ListeningForAnyButtonPress = true;
                return !anyButtonPressed;
            }
        }

        private bool listeningForAnyButtonPress;
        private bool ListeningForAnyButtonPress
        {
            set
            {
                if (listeningForAnyButtonPress == value)
                {
                    return;
                }

                if (ISW.DoesPlayerExist(playerID))
                {
                    if (value) ISW.GetPlayer(playerID).OnAnyButtonPress += HandleAnyButtonPress;
                    else ISW.GetPlayer(playerID).OnAnyButtonPress -= HandleAnyButtonPress;
                }
                else
                {
                    if (value) ISW.OnAnyButtonPress += HandleAnyButtonPress;
                    else ISW.OnAnyButtonPress -= HandleAnyButtonPress;
                }
                    
                listeningForAnyButtonPress = value;
            }
        }

        private readonly int playerID = -1;
        private bool anyButtonPressed;
            
        ~WaitForAnyButtonPress() => ListeningForAnyButtonPress = false;

        /// <summary>
        /// Listen for any button press for any player/device.
        /// </summary>
        public WaitForAnyButtonPress()
        {
            ListeningForAnyButtonPress = true;
        }

        /// <summary>
        /// Listen for any button press for a specific player.
        /// If that player doesn't exist yet, the yield will end immediately, but can be reused again later
        /// after the player has been created to properly wait for their button press.
        /// </summary>
        public WaitForAnyButtonPress(int playerID)
        {
            this.playerID = playerID;
            ListeningForAnyButtonPress = true;
        }

        private void HandleAnyButtonPress(InputControl inputControl)
        {
            anyButtonPressed = true;
        }

        private void ResetYieldInstruction()
        {
            anyButtonPressed = false;
            ListeningForAnyButtonPress = false;
        }
    }
}