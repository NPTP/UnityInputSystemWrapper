using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NPTP.InputSystemWrapper.Player
{
    /// <summary>
    /// Keeps the rest of a cursor's graphics in step with the one the virtual mouse drives. The mouse
    /// enables and disables only that one graphic when it hands drawing over to the hardware cursor, so
    /// anything else in the cursor - a drop shadow, an outline - would otherwise be left on screen.
    /// <para>
    /// Added to the driven graphic's own object, and it enables and disables graphics rather than the
    /// objects holding them, so it keeps running while the cursor is hidden and can bring it back.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    internal sealed class VirtualMouseCursorGraphics : MonoBehaviour
    {
        private Graphic driven;
        private readonly List<Graphic> followers = new();
        private bool drawnLastFrame;

        /// <summary>Match every other graphic in this cursor to the one the mouse drives.</summary>
        internal void Follow(Graphic graphic)
        {
            driven = graphic;

            followers.Clear();
            GetComponentsInChildren(includeInactive: true, followers);
            followers.Remove(driven);

            drawnLastFrame = driven.enabled;
            Apply();
        }

        private void LateUpdate()
        {
            if (driven == null || driven.enabled == drawnLastFrame)
            {
                return;
            }

            drawnLastFrame = driven.enabled;
            Apply();
        }

        private void Apply()
        {
            foreach (Graphic follower in followers)
            {
                if (follower != null)
                {
                    follower.enabled = drawnLastFrame;
                }
            }
        }
    }
}
