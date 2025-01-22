using System.Collections.Generic;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableTypeDefinition : GeneratableBase
    {
        private const int TAB_SPACES_COUNT = 4;
        private const string OPEN_BRACE = "{";
        private const string CLOSE_BRACE = "}";

        protected abstract TypeDefinition TypeDefinition { get; }

        internal List<string> Directives { get; } = new();
        internal string Namespace { get; set; }

        internal InheritanceModifier InheritanceModifier { get; set; }
        internal bool IsPartial { get; set; }
        internal string InheritsFrom { get; set; }

        internal List<GeneratableField> Fields { get; } = new();
        internal List<GeneratableProperty> Properties { get; } = new();
        internal List<GeneratableMethod> Methods { get; } = new();

        internal GeneratableTypeDefinition(string name, AccessModifier accessModifier) : base(name, accessModifier)
        {
            Name = name;
        }

        public override string GenerateStringRepresentation()
        {
            int indent = 0;
            StringBuilder sb = new();

            AddUsingDirectives(sb, indent);
            AddNamespace(sb, indent);
            if (HasNamespace())
            {
                AddOpenBrace(sb, indent);
                indent++;
            }

            AddClassSignature(sb, indent);
            AddOpenBrace(sb, indent);
            
            indent++;
            AddFields(sb, indent);
            // TODO: properties
            // TODO: methods
            indent--;
            
            AddCloseBrace(sb, indent);
            
            if (HasNamespace())
            {
                indent--;
                AddCloseBrace(sb, indent);
            }
            
            return sb.ToString();
        }

        private string Tab(int count)
        {
            StringBuilder tab = new();
            for (int i = 0; i < TAB_SPACES_COUNT * count; i++) tab.Append(SPACE);
            return tab.ToString();
        }

        private void AddLine(StringBuilder sb, int indent, string line)
        {
            sb.AppendLine($"{Tab(indent)}{line}");
        }

        private void AddEmptyLine(StringBuilder sb)
        {
            sb.AppendLine();
        }

        private void AddOpenBrace(StringBuilder sb, int indent)
        {
            AddLine(sb, indent, OPEN_BRACE);
        }

        private void AddCloseBrace(StringBuilder sb, int indent)
        {
            AddLine(sb, indent, CLOSE_BRACE);
        }

        private void AddUsingDirectives(StringBuilder sb, int indent)
        {
            Directives.ForEach(directive => AddLine(sb, indent, $"using {directive};"));
            if (Directives.Count > 0) AddEmptyLine(sb);
        }

        private void AddNamespace(StringBuilder sb, int indent)
        {
            if (!HasNamespace()) return;
            AddLine(sb, indent, $"namespace {Namespace}");
        }

        private bool HasNamespace() => !string.IsNullOrEmpty(Namespace);

        private void AddClassSignature(StringBuilder sb, int indent)
        {
            StringBuilder classSignature = new();
            string accessModifier = AccessModifier.AsString();
            string inheritanceModifier = InheritanceModifier.AsString();
            string partial = IsPartial ? "partial" : string.Empty;
            string objectType = TypeDefinition.AsString();
            string name = Name;
            string inheritsFrom = InheritsFrom;

            classSignature.Append(accessModifier);
            if (!string.IsNullOrEmpty(inheritanceModifier)) classSignature.Append(SPACE + inheritanceModifier);
            if (!string.IsNullOrEmpty(partial)) classSignature.Append(SPACE + partial);
            classSignature.Append(SPACE + objectType);
            classSignature.Append(SPACE + name);
            if (!string.IsNullOrEmpty(inheritsFrom)) classSignature.Append(SPACE + $": {inheritsFrom}");

            AddLine(sb, indent, classSignature.ToString());
        }

        private void AddFields(StringBuilder sb, int indent)
        {
            Fields.ForEach(field => AddLine(sb, indent, field.GenerateStringRepresentation()));
            if (Fields.Count > 0 && (Methods.Count > 0 || Properties.Count > 0)) AddEmptyLine(sb);
        }
    }
}