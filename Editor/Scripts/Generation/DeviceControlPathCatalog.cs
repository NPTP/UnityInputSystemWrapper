using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Enumerates control paths straight out of the input system's own layout registry, rather than from a
    /// hand-written list. Anything Unity knows about is covered, including layouts added by other packages
    /// or by the project itself, and the paths stay correct across input system versions.
    /// </summary>
    internal static class DeviceControlPathCatalog
    {
        /// <summary>
        /// Layouts nest - a Gamepad has a Stick, which has an Axis - but not deeply. This only guards
        /// against a malformed layout referring to itself in a cycle the visited set cannot catch.
        /// </summary>
        private const int MAX_DEPTH = 6;

        /// <summary>
        /// Every control path a device of this layout can produce, relative to the device, mapped to the
        /// display name the input system gives it. Relative is the form binding data is keyed by, since
        /// the runtime strips the "&lt;Device&gt;/" prefix before looking a binding up.
        /// <para>
        /// Paths are ordered as the layout declares them, so a generated asset reads in the same order as
        /// the device's own documentation rather than alphabetically.
        /// </para>
        /// </summary>
        internal static Dictionary<string, string> GetControlPaths(string layoutName)
        {
            Dictionary<string, string> pathsToDisplayNames = new();
            Collect(layoutName, parentPath: string.Empty, new HashSet<string>(), pathsToDisplayNames, depth: 0);
            return pathsToDisplayNames;
        }

        /// <summary>
        /// The device layouts a control scheme requires, e.g. "Gamepad" and "Mouse" for a scheme built from
        /// those two. Optional requirements are included: a binding can still be made against them.
        /// </summary>
        internal static IEnumerable<string> GetRequiredDeviceLayouts(InputControlScheme controlScheme)
        {
            HashSet<string> layouts = new();

            foreach (InputControlScheme.DeviceRequirement requirement in controlScheme.deviceRequirements)
            {
                string layoutName = InputControlPath.TryGetDeviceLayout(requirement.controlPath);
                if (string.IsNullOrEmpty(layoutName))
                {
                    continue;
                }

                // TryGetDeviceLayout can hand back the path's angle brackets along with the name.
                layouts.Add(layoutName.Trim('<', '>'));
            }

            return layouts;
        }

        private static void Collect(string layoutName, string parentPath, HashSet<string> visitedLayouts,
            IDictionary<string, string> pathsToDisplayNames, int depth)
        {
            if (depth >= MAX_DEPTH || string.IsNullOrEmpty(layoutName) || !visitedLayouts.Add(layoutName))
            {
                return;
            }

            InputControlLayout layout = InputSystem.LoadLayout(layoutName);
            if (layout != null)
            {
                foreach (InputControlLayout.ControlItem control in layout.controls)
                {
                    // A modifying item retunes a control its base layout already declared, so it would be
                    // a duplicate rather than a control of its own.
                    if (control.isModifyingExistingControl)
                    {
                        continue;
                    }

                    string controlName = control.name;
                    if (string.IsNullOrEmpty(controlName))
                    {
                        continue;
                    }

                    string path = string.IsNullOrEmpty(parentPath) ? controlName : $"{parentPath}/{controlName}";
                    pathsToDisplayNames[path] = string.IsNullOrEmpty(control.displayName) ? controlName : control.displayName;

                    // The control's own layout describes its children, e.g. a Stick's x, y, up and down.
                    Collect(control.layout, path, visitedLayouts, pathsToDisplayNames, depth + 1);
                }
            }

            // Removed rather than left in place, so the same layout can appear under two different parents,
            // e.g. Stick under both leftStick and rightStick.
            visitedLayouts.Remove(layoutName);
        }
    }
}
