using UnityEngine;

namespace NPTP.InputSystemWrapper.Attributes
{
    /// <summary>
    /// Draws a string field as a searchable dropdown of every control path the input system recognizes,
    /// grouped by device. The underlying value is still just the path string, so nothing about how it is
    /// stored or used changes - this only stops a designer having to type one from memory.
    /// <para>
    /// Paths already chosen elsewhere in the same list are left out of the dropdown, so the same control
    /// cannot be added twice.
    /// </para>
    /// </summary>
    public class ControlPathSelectorAttribute : PropertyAttribute
    {
    }
}
