using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// The root of a virtual mouse cursor. Put it on a cursor prefab, point it at the graphic the mouse
    /// should move, and name that prefab on the input data.
    /// <para>
    /// The mouse shows and hides only the graphic it is given, so anything else in the cursor - a drop
    /// shadow, an outline - is kept in step with it here. Graphics are enabled and disabled rather than
    /// the objects holding them, so this keeps running while the cursor is hidden and can bring it back.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class ISWVirtualMouseUI : MonoBehaviour
    {
        [Tooltip("The graphic the virtual mouse moves. Its pivot is the point that clicks, so put it on the " +
                 "cursor's tip, and anchor it to the bottom left.")]
        [SerializeField] private Graphic cursorGraphic;

        /// <summary>The graphic the mouse moves and draws its position with.</summary>
        public Graphic CursorGraphic => cursorGraphic;

        /// <summary>Everything else drawn in this cursor, shown and hidden along with the graphic above.</summary>
        private readonly List<Graphic> followers = new();

        private bool drawnLastFrame;

        private void Awake()
        {
            GetComponentsInChildren(includeInactive: true, followers);
            followers.Remove(cursorGraphic);

            if (cursorGraphic == null)
            {
                return;
            }

            drawnLastFrame = cursorGraphic.enabled;
            Apply();
        }

        private void LateUpdate()
        {
            if (cursorGraphic == null || cursorGraphic.enabled == drawnLastFrame)
            {
                return;
            }

            drawnLastFrame = cursorGraphic.enabled;
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
