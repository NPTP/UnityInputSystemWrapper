using System;
using System.Collections.Generic;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public sealed class GeneratableEnum : GeneratableBase
    {
        internal class Member
        {
            internal enum EnumValueMode
            {
                NonExplicit,
                ExplicitInt,
                ExplicitBitShiftFlag
            }
            
            internal string Name { get; }
            internal EnumValueMode ValueMode { get; }
            internal int Value { get; }
            internal int BitShiftValue { get; }
            
            internal Member(string name, EnumValueMode valueMode, int? value, int? bitShiftValue)
            {
                Name = name;
                ValueMode = valueMode;
                if (value.HasValue) Value = value.Value;
                if (bitShiftValue.HasValue) BitShiftValue = bitShiftValue.Value;
            }

            public override string ToString()
            {
                return ValueMode switch
                {
                    EnumValueMode.NonExplicit => Name,
                    EnumValueMode.ExplicitInt => $"{Name} = {Value}",
                    EnumValueMode.ExplicitBitShiftFlag => $"{Name} = {Value} << {BitShiftValue}",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        private List<Member> Members { get; } = new();
        
        public GeneratableEnum(string name, AccessModifier accessModifier) : base(name, accessModifier) { }

        internal void AddMember(string name, Member.EnumValueMode valueMode, int? value, int? bitShiftValue)
        {
            Members.Add(new Member(name, valueMode, value, bitShiftValue));
        }

        public override string GenerateStringRepresentation()
        {
            // TODO: Share members with GeneratedObjectDefinition to simplify this. Tabs, etc.
            StringBuilder sb = new();

            sb.AppendLine($"{AccessModifier.AsString()} enum {Name}");
            sb.AppendLine("{");
            for (int i = 0; i < Members.Count; i++)
            {
                sb.Append($"    {Members[i]}");
                if (i < Members.Count - 1) {sb.Append(',');}
                sb.Append(Environment.NewLine);
            }
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}