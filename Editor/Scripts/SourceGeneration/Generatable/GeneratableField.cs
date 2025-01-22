using System;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableField : GeneratableBase
    {
        protected GeneratableField(string name, AccessModifier accessModifier) : base(name, accessModifier) { }
    }
    
    public class GeneratableField<T> : GeneratableField
    {
        private readonly bool hasInitialValue;
        private readonly T initialValue;

        private static Type FieldType => typeof(T);
        
        public GeneratableField(string name, AccessModifier accessModifier) : base(name, accessModifier)
        {
            hasInitialValue = false;
        }

        public GeneratableField(string name, AccessModifier accessModifier, T initialValue) : base(name, accessModifier)
        {
            this.initialValue = initialValue;
            hasInitialValue = true;
        }

        public override string GenerateStringRepresentation()
        {
            StringBuilder field = new();
            field.Append(AccessModifier.AsString());
            PrependAdditionalLabels(field);
            field.Append(SPACE + GetTypeName());
            field.Append(SPACE + Name);
            if (TryGetInitialValueAsString(out string initialValueString))
            {
                field.Append(SPACE + "=" + SPACE);
                field.Append(initialValueString);

            }
            field.Append(SEMICOLON);

            return field.ToString();
        }

        protected virtual void PrependAdditionalLabels(StringBuilder fieldStringBuilder) { }

        private bool TryGetInitialValueAsString(out string initialValueString)
        {
            if (!hasInitialValue)
            {
                initialValueString = null;
                return false;
            }
            
            StringBuilder sb = new();
            if (FieldType == typeof(string)) sb.Append('"');
            sb.Append(initialValue);
            if (FieldType == typeof(string)) sb.Append('"');
            
            initialValueString = sb.ToString();
            return true;
        }

        private string GetTypeName()
        {
            if (FieldType == typeof(string))	
                return "string";
            if (FieldType == typeof(int))
                return "int";
            if (FieldType == typeof(bool))	
                return "bool";
            if (FieldType == typeof(byte))	
                return "byte";
            if (FieldType == typeof(sbyte))	
                return "sbyte";
            if (FieldType == typeof(char))	
                return "char";
            if (FieldType == typeof(decimal))	
                return "decimal";
            if (FieldType == typeof(double))	
                return "double";
            if (FieldType == typeof(float))	
                return "float";
            if (FieldType == typeof(int))	
                return "int";
            if (FieldType == typeof(uint))	
                return "uint";
            if (FieldType == typeof(nint))	
                return "nint";
            if (FieldType == typeof(nuint))	
                return "nuint";
            if (FieldType == typeof(long))	
                return "long";
            if (FieldType == typeof(ulong))	
                return "ulong";
            if (FieldType == typeof(short))	
                return "short";
            if (FieldType == typeof(ushort))
                return "ushort";

            return FieldType.Name;
        }
    }
}