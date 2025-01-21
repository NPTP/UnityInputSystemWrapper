namespace NPTP.InputSystemWrapper.Editor.SourceGeneration
{
    public sealed class GeneratedClass : GeneratedObjectDefinition
    {
        protected override ObjectType ObjectType => ObjectType.Class;
        
        public GeneratedClass(string name, AccessModifier accessModifier) : base(name, accessModifier)
        {
        }
    }
}
