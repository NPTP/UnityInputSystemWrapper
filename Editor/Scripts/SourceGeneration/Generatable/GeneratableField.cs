using System;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableField : GeneratableBase
    {
        protected GeneratableField(string name, AccessModifier accessModifier, bool isStatic) : base(name, accessModifier, isStatic) { }
    }
    
    public class GeneratableField<T> : GeneratableField
    {
        private readonly bool hasInitialValue;
        private readonly T initialValue;

        private static Type FieldType => typeof(T);
        
        internal GeneratableField(string name, AccessModifier accessModifier, bool isStatic) : base(name, accessModifier, isStatic)
        {
            hasInitialValue = false;
        }

        internal GeneratableField(string name, AccessModifier accessModifier, bool isStatic, T initialValue) : base(name, accessModifier, isStatic)
        {
            this.initialValue = initialValue;
            hasInitialValue = true;
        }

        public override string GenerateStringRepresentation()
        {
            StringBuilder field = new();
            field.Append(AccessModifier.AsString());
            PrependAdditionalLabels(field);
            field.Append(SPACE + GetTypeName(FieldType));
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
            string left = string.Empty;
            string right = string.Empty;

            if (FieldType == typeof(string))
            {
                left = right = "\"";
            }
            else if (FieldType == typeof(float))
            {
                right = "f";
            }

            sb.Append(left);
            sb.Append(initialValue);
            sb.Append(right);
            
            initialValueString = sb.ToString();
            return true;
        }
    }
}