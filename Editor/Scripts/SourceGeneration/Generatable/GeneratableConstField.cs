using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public class GeneratableConstField<T> : GeneratableField<T>
    {
        private const string CONST = "const";
        
        public GeneratableConstField(string name, AccessModifier accessModifier, T initialValue) : base(name, accessModifier, initialValue) { }
        
        protected override void PrependAdditionalLabels(StringBuilder fieldStringBuilder)
        {
            fieldStringBuilder.Append(SPACE + CONST);
        }
    }
}