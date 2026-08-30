using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Reports bindings a rebinding screen can never show. A rebinding screen lists an action's bindings
    /// for one control scheme at a time, so a binding belonging to no scheme never appears on any of them
    /// while still firing its action - a player would be triggering something they cannot see or change.
    /// </summary>
    internal static class BindingAudit
    {
        internal static void Run(InputActionAsset asset)
        {
            List<string> unreachable = new();

            foreach (InputAction action in asset)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];

                    // A composite carries its groups on its parts, so the composite itself is never the
                    // one to report.
                    if (binding.isComposite || !string.IsNullOrEmpty(binding.groups))
                    {
                        continue;
                    }

                    unreachable.Add($"{action.actionMap.name}/{action.name}: {binding.effectivePath}");
                }
            }

            if (unreachable.Count == 0)
            {
                return;
            }

            GenerationReport.Record($"{unreachable.Count} binding(s) belong to no control scheme and will not appear on any " +
                                    $"rebinding screen, though they still fire their action:\n  {string.Join("\n  ", unreachable)}");
        }
    }
}
