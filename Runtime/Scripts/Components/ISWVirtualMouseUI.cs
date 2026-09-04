using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NPTP.InputSystemWrapper.Components
{
    /// <summary>
    /// The root of a virtual mouse cursor. Put it on a cursor prefab, point it at the graphic the mouse
    /// should move, and name that prefab on the input data.
    /// Components are null-validated before instantiation so we can avoidant unperformant UnityEngine.Object
    /// null checks in our per-frame polling.
    /// </summary>
    [DisallowMultipleComponent]
    public class ISWVirtualMouseUI : MonoBehaviour
    {
        [SerializeField] private Graphic cursorGraphic;
        public Graphic CursorGraphic => cursorGraphic;

        [Tooltip("The transform the virtual mouse actually moves.")]
        [SerializeField] private RectTransform cursorTransform;
        public RectTransform CursorTransform => cursorTransform;

        private readonly List<Graphic> cursorGraphicFollowers = new();
        private bool drawnLastFrame;

        private void Awake()
        {
            GetComponentsInChildren(includeInactive: true, cursorGraphicFollowers);
            cursorGraphicFollowers.Remove(cursorGraphic);

            drawnLastFrame = cursorGraphic.enabled;
            Apply();
        }

        private void LateUpdate()
        {
            if (cursorGraphic.enabled == drawnLastFrame)
            {
                return;
            }

            drawnLastFrame = cursorGraphic.enabled;
            Apply();
        }

        private void Apply()
        {
            foreach (Graphic follower in cursorGraphicFollowers)
            {
                follower.enabled = drawnLastFrame;
            }
        }
    }
}
