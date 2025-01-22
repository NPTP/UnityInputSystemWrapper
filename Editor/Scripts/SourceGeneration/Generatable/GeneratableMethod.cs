using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public class GeneratableMethod : GeneratableBase
    {
        internal GeneratableMethod(string name, AccessModifier accessModifier, bool isStatic, InheritanceModifier inheritanceModifier) : base(name, accessModifier)
        {
        }

        public override string GenerateStringRepresentation()
        {
            throw new System.NotImplementedException();
        }
    }
}