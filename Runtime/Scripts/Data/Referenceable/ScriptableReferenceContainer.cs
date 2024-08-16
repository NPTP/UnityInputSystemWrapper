using UnityEngine;

namespace NPTP.InputSystemWrapper.Data.Referenceable
{
    public class ScriptableReferenceContainer : ScriptableObject
    {
        [SerializeField] private ScriptableObject scriptableObjectReference;
        public ScriptableObject ScriptableObjectReference => scriptableObjectReference;

#if UNITY_EDITOR
        public void EDITOR_SetScriptableObjectReference(ScriptableObject scriptableObject)
        {
            scriptableObjectReference = scriptableObject;
        }
#endif
    }
}