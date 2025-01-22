using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableBase
    {
        protected const char SPACE = ' ';
        protected const char SEMICOLON = ';';

        internal AccessModifier AccessModifier { get; set; }
        internal string Name { get; set; }

        protected GeneratableBase(string name, AccessModifier accessModifier)
        {
            Name = name;
            AccessModifier = accessModifier;
        }

        public abstract string GenerateStringRepresentation();
    }
}