namespace NPTP.InputSystemWrapper.Editor.SourceGeneration
{
    public static class SourceGenUtility
    {
        public static GeneratedClass NewGeneratedClass(string name, AccessModifier accessModifier) => new GeneratedClass(name, accessModifier);

        public static GeneratedObjectDefinition WithAccessModifier(this GeneratedObjectDefinition gen, AccessModifier accessModifier)
        {
            gen.AccessModifier = accessModifier;
            return gen;
        }
        
        public static GeneratedObjectDefinition WithDirective(this GeneratedObjectDefinition gen, string directive)
        {
            gen.Directives.Add(directive);
            return gen;
        }

        public static GeneratedObjectDefinition WithDirectives(this GeneratedObjectDefinition gen, params string[] directives)
        {
            foreach (string directive in directives)
            {
                gen.Directives.Add(directive);
            }

            return gen;
        }

        public static GeneratedObjectDefinition InNamespace(this GeneratedObjectDefinition gen, string @namespace)
        {
            gen.Namespace = @namespace;
            return gen;
        }

        public static GeneratedObjectDefinition WithInheritanceModifier(this GeneratedObjectDefinition gen, InheritanceModifier inheritanceModifier)
        {
            gen.InheritanceModifier = inheritanceModifier;
            return gen;
        }

        public static GeneratedObjectDefinition AsPartial(this GeneratedObjectDefinition gen)
        {
            gen.IsPartial = true;
            return gen;
        }
        
        public static GeneratedObjectDefinition InheritsFrom<T>(this GeneratedObjectDefinition gen)
        {
            gen.InheritsFrom = typeof(T).Name;
            return gen;
        }

        public static GeneratedObjectDefinition WithConstField<T>(this GeneratedObjectDefinition gen, string fieldName, AccessModifier accessModifier, T initialValue)
        {
            GeneratedField constField = new GeneratedField(fieldName, accessModifier, isConst: true, typeof(T));
            constField.SetInitialValue(initialValue);
            gen.Fields.Add(constField);
            return gen;
        }

        public static GeneratedObjectDefinition WithField<T>(this GeneratedObjectDefinition gen, string fieldName, AccessModifier accessModifier)
        {
            gen.Fields.Add(new GeneratedField(fieldName, accessModifier, isConst: false, typeof(T)));
            return gen;
        }

        public static GeneratedObjectDefinition WithProperty(this GeneratedObjectDefinition gen, string propertyName, AccessModifier getModifier, AccessModifier setModifier)
        {
            gen.Properties.Add(new GeneratedProperty(propertyName, getModifier, setModifier));
            return gen;
        }

        public static GeneratedObjectDefinition WithStaticMethod(this GeneratedObjectDefinition gen, string methodName, AccessModifier accessModifier)
        {
            gen.Methods.Add(new GeneratedMethod(methodName, accessModifier, isStatic: true, InheritanceModifier.None));
            return gen;
        }
        
        public static GeneratedObjectDefinition WithMethod(this GeneratedObjectDefinition gen, string methodName, AccessModifier accessModifier, InheritanceModifier inheritanceModifier)
        {
            gen.Methods.Add(new GeneratedMethod(methodName, accessModifier, isStatic: false, inheritanceModifier));
            return gen;
        }
    }
}
