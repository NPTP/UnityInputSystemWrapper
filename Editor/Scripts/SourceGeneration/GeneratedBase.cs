namespace NPTP.InputSystemWrapper.Editor.SourceGeneration
{
    public abstract class GeneratedBase
    {
        protected const char SPACE = ' ';
        protected const char SEMICOLON = ';';

        internal AccessModifier AccessModifier { get; set; }
        internal string Name { get; set; }

        protected GeneratedBase(string name, AccessModifier accessModifier)
        {
            Name = name;
            AccessModifier = accessModifier;
        }
    }
}