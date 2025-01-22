using System.Collections.Generic;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableDefinition : GeneratableBase
    {
        private const int TAB_SPACES_COUNT = 4;
        private const string OPEN_BRACE = "{";
        private const string CLOSE_BRACE = "}";
        protected const string COMMA = ",";
        
        // TODO: Adding directives, fields, types etc should auto-add directives
        internal SortedSet<string> Directives { get; } = new();
        internal string Namespace { get; set; }

        protected GeneratableDefinition(string name, AccessModifier accessModifier) : base(name, accessModifier) { }

        protected void AddLine(StringBuilder sb, int indent, string line) => sb.AppendLine(Tab(indent) + line);

        protected void AddEmptyLine(StringBuilder sb) => sb.AppendLine();

        protected void AddOpenBrace(StringBuilder sb, int indent) => AddLine(sb, indent, OPEN_BRACE);

        protected void AddCloseBrace(StringBuilder sb, int indent) => AddLine(sb, indent, CLOSE_BRACE);

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
        
        private string Tab(int count)
        {
            StringBuilder tab = new();
            for (int i = 0; i < TAB_SPACES_COUNT * count; i++) tab.Append(SPACE);
            return tab.ToString();
        }
    }
}