using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public class GeneratableProperty : GeneratableBase
    {
        internal GeneratableProperty(string name, AccessModifier getModifier, AccessModifier setModifier) : base(name, getModifier)
        {
        }

        public override string GenerateStringRepresentation()
        {
            throw new System.NotImplementedException();
        }
    }
}