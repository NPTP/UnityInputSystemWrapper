using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public sealed class GeneratableClass : GeneratableTypeDefinition
    {
        protected override TypeDefinition TypeDefinition => TypeDefinition.Class;
        
        internal GeneratableClass(string name, AccessModifier accessModifier, bool isStatic) : base(name, accessModifier, isStatic)
        {
        }
    }
}
