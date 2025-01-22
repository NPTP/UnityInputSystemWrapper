using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public sealed class GeneratableStruct : GeneratableTypeDefinition
    {
        protected override TypeDefinition TypeDefinition => TypeDefinition.Struct;

        public GeneratableStruct(string name, AccessModifier accessModifier) : base(name, accessModifier)
        {
        }
    }
}
