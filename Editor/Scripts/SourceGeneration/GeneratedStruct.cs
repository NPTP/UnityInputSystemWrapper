namespace NPTP.InputSystemWrapper.Editor.SourceGeneration
{
    public sealed class GeneratedStruct : GeneratedObjectDefinition
    {
        protected override ObjectType ObjectType => ObjectType.Struct;

        public GeneratedStruct(string name, AccessModifier accessModifier) : base(name, accessModifier)
        {
        }
    }
}
