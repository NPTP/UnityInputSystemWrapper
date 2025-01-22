using System.Collections.Generic;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableTypeDefinition : GeneratableDefinition
    {
        protected abstract TypeDefinition TypeDefinition { get; }

        internal InheritanceModifier InheritanceModifier { get; set; }
        internal bool IsPartial { get; set; }
        internal string InheritsFrom { get; set; }
        internal SortedSet<string> ImplementsInterfaces { get; } = new();

        // TODO: Adding fields or properties with the same name should override any existing ones
        internal List<GeneratableField> Fields { get; } = new();
        internal List<GeneratableProperty> Properties { get; } = new();
        internal List<GeneratableMethod> Methods { get; } = new();

        internal GeneratableTypeDefinition(string name, AccessModifier accessModifier) : base(name, accessModifier) { }

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
            AddProperties(sb, indent);
            AddMethods(sb, indent);
            
            indent--;
            
            AddCloseBrace(sb, indent);
            
            if (HasNamespace())
            {
                indent--;
                AddCloseBrace(sb, indent);
            }
            
            return sb.ToString();
        }

        private void AddClassSignature(StringBuilder sb, int indent)
        {
            StringBuilder classSignature = new();
            string accessModifier = AccessModifier.AsString();
            string inheritanceModifier = InheritanceModifier.AsString();
            string partial = IsPartial ? "partial" : string.Empty;
            string typeDefinition = TypeDefinition.AsString();
            string name = Name;
            string inheritsFrom = InheritsFrom;

            classSignature.Append(accessModifier);
            if (!string.IsNullOrEmpty(inheritanceModifier)) classSignature.Append(SPACE + inheritanceModifier);
            if (!string.IsNullOrEmpty(partial)) classSignature.Append(SPACE + partial);
            classSignature.Append(SPACE + typeDefinition);
            classSignature.Append(SPACE + name);

            bool inheritsFromSomething = !string.IsNullOrEmpty(inheritsFrom);
            bool implementsInterfaces = ImplementsInterfaces.Count > 0;
            if (inheritsFromSomething || implementsInterfaces)
            {
                classSignature.Append(SPACE + ':' + SPACE);
                if (inheritsFromSomething)
                {
                    classSignature.Append(inheritsFrom);
                    if (implementsInterfaces) classSignature.Append(COMMA + SPACE);
                }
                
                if (implementsInterfaces)
                {
                    int i = 0;
                    foreach (string implementsInterface in ImplementsInterfaces)
                    {
                        classSignature.Append(implementsInterface + (i < ImplementsInterfaces.Count - 1 ? COMMA + SPACE : string.Empty));
                        i++;
                    }
                }
            }

            AddLine(sb, indent, classSignature.ToString());
        }

        private void AddFields(StringBuilder sb, int indent)
        {
            Fields.ForEach(field => AddLine(sb, indent, field.GenerateStringRepresentation()));
            if (Fields.Count > 0 && (Methods.Count > 0 || Properties.Count > 0)) AddEmptyLine(sb);
        }
        
        private void AddProperties(StringBuilder sb, int indent)
        {
            // TODO
        }

        private void AddMethods(StringBuilder sb, int indent)
        {
            // TODO
        }
    }
}