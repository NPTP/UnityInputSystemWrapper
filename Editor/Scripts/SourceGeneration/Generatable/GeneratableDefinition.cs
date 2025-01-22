using System.Collections.Generic;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableDefinition : GeneratableBase
    {
        protected const string COMMA = ",";
        
        // TODO: Adding directives, fields, types etc should auto-add directives
        internal SortedSet<string> Directives { get; } = new();
        internal string Namespace { get; set; }

        internal GeneratableDefinition(string name, AccessModifier accessModifier, bool isStatic) : base(name, accessModifier, isStatic) { }

        protected void AddUsingDirectives(StringBuilder sb, int indent)
        {
            foreach (string directive in Directives)
                AddLine(sb, indent, $"using {directive};");
            
            if (Directives.Count > 0)
                AddEmptyLine(sb);
        }

        protected void AddNamespace(StringBuilder sb, int indent)
        {
            if (!HasNamespace()) return;
            AddLine(sb, indent, $"namespace {Namespace}");
        }

        protected bool HasNamespace() => !string.IsNullOrEmpty(Namespace);

    }
}