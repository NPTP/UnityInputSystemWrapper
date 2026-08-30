using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Reports bindings belonging to no control scheme. A rebinding screen lists one scheme at a time,
    /// so those never appear on any of them while still firing their action.
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

                    // A composite carries its groups on its parts, never on itself.
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
