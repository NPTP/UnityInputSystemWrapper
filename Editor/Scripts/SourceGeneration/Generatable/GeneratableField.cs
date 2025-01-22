using System;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public class GeneratableField : GeneratableBase
    {
        private const string CONST = "const";
        
        private readonly bool isConst;
        private readonly Type fieldType;

        private string initialValueString;

        // TODO: Support setting an initial value
        public GeneratableField(string name, AccessModifier accessModifier, bool isConst, Type fieldType) : base(name, accessModifier)
        {
            this.isConst = isConst;
            this.fieldType = fieldType;
        }

        public override string GenerateStringRepresentation()
        {
            StringBuilder field = new();
            field.Append(AccessModifier.AsString());
            if (isConst) field.Append(SPACE + CONST);
            field.Append(SPACE + GetTypeName());
            field.Append(SPACE + Name);
            if (!string.IsNullOrEmpty(initialValueString)) field.Append($" = {initialValueString}");
            field.Append(SEMICOLON);

            return field.ToString();
        }
        
        public void SetInitialValue<T>(T value)
        {
            if (typeof(T) != fieldType)
            {
                return;
            }

            initialValueString = value.ToString();
        }
        
        private string GetTypeName()
        {
            if (fieldType == typeof(string))	
                return "string";
            if (fieldType == typeof(int))
                return "int";
            if (fieldType == typeof(bool))	
                return "bool";
            if (fieldType == typeof(byte))	
                return "byte";
            if (fieldType == typeof(sbyte))	
                return "sbyte";
            if (fieldType == typeof(char))	
                return "char";
            if (fieldType == typeof(decimal))	
                return "decimal";
            if (fieldType == typeof(double))	
                return "double";
            if (fieldType == typeof(float))	
                return "float";
            if (fieldType == typeof(int))	
                return "int";
            if (fieldType == typeof(uint))	
                return "uint";
            if (fieldType == typeof(nint))	
                return "nint";
            if (fieldType == typeof(nuint))	
                return "nuint";
            if (fieldType == typeof(long))	
                return "long";
            if (fieldType == typeof(ulong))	
                return "ulong";
            if (fieldType == typeof(short))	
                return "short";
            if (fieldType == typeof(ushort))
                return "ushort";

            return fieldType.Name;
        }
    }
}