using System.Collections.Generic;
using System.Linq;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableTypeDefinition : GeneratableDefinition
    {
        private const string PARTIAL = "partial";
        
        protected abstract TypeDefinition TypeDefinition { get; }

        internal InheritanceModifier InheritanceModifier { get; set; }
        internal bool IsPartial { get; set; }
        internal string InheritsFrom { get; set; }
        internal SortedSet<string> ImplementsInterfaces { get; } = new();

        // TODO: Adding fields or properties with the same name should override any existing ones
        private List<GeneratableField> Fields { get; } = new();
        private List<GeneratableProperty> Properties { get; } = new();
        private List<GeneratableMethod> Methods { get; } = new();

        internal GeneratableTypeDefinition(string name, AccessModifier accessModifier, bool isStatic) : base(name, accessModifier, isStatic) { }

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

            classSignature.Append(AccessModifier.AsString());
            if (InheritanceModifier != InheritanceModifier.None) classSignature.Append(SPACE + InheritanceModifier.AsString());
            if (IsPartial) classSignature.Append(SPACE + PARTIAL);
            classSignature.Append(SPACE + TypeDefinition.AsString());
            classSignature.Append(SPACE + Name);

            bool inheritsFromSomething = !string.IsNullOrEmpty(InheritsFrom);
            bool implementsInterfaces = ImplementsInterfaces.Count > 0;
            if (inheritsFromSomething || implementsInterfaces)
            {
                classSignature.Append(SPACE + ":" + SPACE);
                if (inheritsFromSomething)
                {
                    classSignature.Append(InheritsFrom);
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
            Properties.ForEach(property => AddLine(sb, indent, property.GenerateStringRepresentation()));
            if (Properties.Count > 0 && Methods.Count > 0) AddEmptyLine(sb);
        }

        private void AddMethods(StringBuilder sb, int indent)
        {
            int i = 0;
            foreach (GeneratableMethod method in Methods)
            {
                AddLines(sb, indent, method.GenerateStringRepresentationLines());
                if (i < Methods.Count - 1) AddEmptyLine(sb);
                i++;
            }
        }

        internal void AddField(GeneratableField field) => Add(field, Fields);
        internal void AddProperty(GeneratableProperty property) => Add(property, Properties);
        internal void AddMethod(GeneratableMethod method) => Add(method, Methods);
        
        private void Add<T>(T generatable, List<T> generatableList) where T : GeneratableBase
        {
            if (generatableList.Any(generatableElement => generatable.Name == generatableElement.Name))
            {
                return;
            }

            generatableList.Add(generatable);
        }
    }
}