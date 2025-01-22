using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration
{
    public static class SourceGenerator
    {
        #region Generated Enum
        
        public static GeneratableEnum NewGeneratedEnum(string name, AccessModifier accessModifier) => new GeneratableEnum(name, accessModifier);

        public static GeneratableEnum WithMember(this GeneratableEnum gen, string name)
        {
            gen.AddMember(name, GeneratableEnum.Member.EnumValueMode.NonExplicit, null, null);
            return gen;
        } 
        
        public static GeneratableEnum WithIntValuedMember(this GeneratableEnum gen, string name, int value)
        {
            gen.AddMember(name, GeneratableEnum.Member.EnumValueMode.ExplicitInt, value, null);
            return gen;
        }
        
        public static GeneratableEnum WithBitShiftedMember(this GeneratableEnum gen, string name, int value, int bitShiftValue)
        {
            gen.AddMember(name, GeneratableEnum.Member.EnumValueMode.ExplicitBitShiftFlag, value, bitShiftValue);
            return gen;
        }
        
        #endregion

        #region Generated Object Definition

        public static GeneratableClass NewGeneratedClass(string name, AccessModifier accessModifier) => new GeneratableClass(name, accessModifier);

        public static GeneratableTypeDefinition WithAccessModifier(this GeneratableTypeDefinition gen, AccessModifier accessModifier)
        {
            gen.AccessModifier = accessModifier;
            return gen;
        }
        
        public static GeneratableTypeDefinition WithDirective(this GeneratableTypeDefinition gen, string directive)
        {
            gen.Directives.Add(directive);
            return gen;
        }

        public static GeneratableTypeDefinition WithDirectives(this GeneratableTypeDefinition gen, params string[] directives)
        {
            foreach (string directive in directives)
            {
                gen.Directives.Add(directive);
            }

            return gen;
        }

        public static GeneratableTypeDefinition InNamespace(this GeneratableTypeDefinition gen, string @namespace)
        {
            gen.Namespace = @namespace;
            return gen;
        }

        public static GeneratableTypeDefinition WithInheritanceModifier(this GeneratableTypeDefinition gen, InheritanceModifier inheritanceModifier)
        {
            gen.InheritanceModifier = inheritanceModifier;
            return gen;
        }

        public static GeneratableTypeDefinition AsPartial(this GeneratableTypeDefinition gen)
        {
            gen.IsPartial = true;
            return gen;
        }
        
        public static GeneratableTypeDefinition InheritsFrom<T>(this GeneratableTypeDefinition gen)
        {
            gen.InheritsFrom = typeof(T).Name;
            return gen;
        }

        public static GeneratableTypeDefinition WithConstField<T>(this GeneratableTypeDefinition gen, string fieldName, AccessModifier accessModifier, T initialValue)
        {
            GeneratableField constField = new GeneratableField(fieldName, accessModifier, isConst: true, typeof(T));
            constField.SetInitialValue(initialValue);
            gen.Fields.Add(constField);
            return gen;
        }

        public static GeneratableTypeDefinition WithField<T>(this GeneratableTypeDefinition gen, string fieldName, AccessModifier accessModifier)
        {
            gen.Fields.Add(new GeneratableField(fieldName, accessModifier, isConst: false, typeof(T)));
            return gen;
        }

        public static GeneratableTypeDefinition WithProperty(this GeneratableTypeDefinition gen, string propertyName, AccessModifier getModifier, AccessModifier setModifier)
        {
            gen.Properties.Add(new GeneratableProperty(propertyName, getModifier, setModifier));
            return gen;
        }

        public static GeneratableTypeDefinition WithStaticMethod(this GeneratableTypeDefinition gen, string methodName, AccessModifier accessModifier)
        {
            gen.Methods.Add(new GeneratableMethod(methodName, accessModifier, isStatic: true, InheritanceModifier.None));
            return gen;
        }
        
        public static GeneratableTypeDefinition WithMethod(this GeneratableTypeDefinition gen, string methodName, AccessModifier accessModifier, InheritanceModifier inheritanceModifier)
        {
            gen.Methods.Add(new GeneratableMethod(methodName, accessModifier, isStatic: false, inheritanceModifier));
            return gen;
        }
        
        #endregion
    }
}
