using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration
{
    public static class SourceGenerator
    {
        #region Generalized
        
        public static T InNamespace<T>(this T gen, string @namespace) where T : GeneratableDefinition
        {
            gen.Namespace = @namespace;
            return gen;
        }
        
        public static T WithAccessModifier<T>(this T gen, AccessModifier accessModifier) where T : GeneratableDefinition
        {
            gen.AccessModifier = accessModifier;
            return gen;
        }
        
        public static T WithDirective<T>(this T gen, string directive) where T : GeneratableDefinition
        {
            gen.Directives.Add(directive);
            return gen;
        }

        public static T WithDirectives<T>(this T gen, params string[] directives) where T : GeneratableDefinition
        {
            foreach (string directive in directives)
            {
                gen.Directives.Add(directive);
            }

            return gen;
        }

        #endregion
        
        #region Generatable Enum
        
        public static GeneratableEnum NewGeneratedEnum(string name, AccessModifier accessModifier) => new GeneratableEnum(name, accessModifier);

        public static GeneratableEnum AsFlags(this GeneratableEnum gen)
        {
            gen.IsFlags = true;
            return gen;
        }
        
        public static GeneratableEnum WithMember(this GeneratableEnum gen, string name)
        {
            gen.AddMember(name, GeneratableEnum.EnumMember.EnumValueMode.NonExplicit, null, null);
            return gen;
        } 
        
        public static GeneratableEnum WithIntValuedMember(this GeneratableEnum gen, string name, int value)
        {
            gen.AddMember(name, GeneratableEnum.EnumMember.EnumValueMode.ExplicitInt, value, null);
            return gen;
        }
        
        public static GeneratableEnum WithBitShiftedMember(this GeneratableEnum gen, string name, int value, int bitShiftValue)
        {
            gen.AddMember(name, GeneratableEnum.EnumMember.EnumValueMode.ExplicitBitShiftFlag, value, bitShiftValue);
            return gen;
        }
        
        #endregion

        #region Generatable Type Definition

        public static GeneratableClass NewGeneratedClass(string name, AccessModifier accessModifier) => new GeneratableClass(name, accessModifier);

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
        
        public static GeneratableTypeDefinition ImplementsInterface<T>(this GeneratableTypeDefinition gen) where T : class
        {
            gen.ImplementsInterfaces.Add(typeof(T).Name);
            return gen;
        }

        public static GeneratableTypeDefinition WithConstField<T>(this GeneratableTypeDefinition gen, string fieldName, AccessModifier accessModifier, T initialValue)
        {
            gen.Fields.Add(new GeneratableConstField<T>(fieldName, accessModifier, initialValue));
            return gen;
        }
        
        public static GeneratableTypeDefinition WithField<T>(this GeneratableTypeDefinition gen, string fieldName, AccessModifier accessModifier)
        {
            gen.Fields.Add(new GeneratableField<T>(fieldName, accessModifier));
            return gen;
        }

        public static GeneratableTypeDefinition WithField<T>(this GeneratableTypeDefinition gen, string fieldName, AccessModifier accessModifier, T initialValue)
        {
            gen.Fields.Add(new GeneratableField<T>(fieldName, accessModifier, initialValue));
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
