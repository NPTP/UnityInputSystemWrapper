using UnityEngine;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper
{
    public static partial class Input
    {
        /// <summary>
        /// Custom yield instruction for coroutines to make waiting for any button press a lot more syntactically convenient.
        /// Use like:
        /// yield return new Input.WaitForAnyButtonPress();
        /// </summary>
        public class WaitForAnyButtonPress : CustomYieldInstruction
        {
            public override bool keepWaiting
            {
                get
                {
                    if (anyButtonPressed)
                    {
                        ResetYieldInstruction();
                        return false;
                    }

                    if (!ListeningForAnyButtonPress)
                    {
                        ListeningForAnyButtonPress = true;
                    }
                    
                    return !anyButtonPressed;
                }
            }

            private bool listeningForAnyButtonPress;
            private bool ListeningForAnyButtonPress
            {
                get => listeningForAnyButtonPress;
                set
                {
                    if (listeningForAnyButtonPress == value)
                        return;
                    
                    if (value) OnAnyButtonPress += HandleAnyButtonPress;
                    else OnAnyButtonPress -= HandleAnyButtonPress;
                    listeningForAnyButtonPress = value;
                }
            }
            
            private bool anyButtonPressed;
            
            ~WaitForAnyButtonPress() => OnAnyButtonPress -= HandleAnyButtonPress;

            public WaitForAnyButtonPress()
            {
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
}