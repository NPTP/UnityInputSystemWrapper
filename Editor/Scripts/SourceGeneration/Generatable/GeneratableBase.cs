using System;
using System.Collections.Generic;
using System.Text;
using NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Generatable
{
    public abstract class GeneratableBase
    {
        protected const string SPACE = " ";
        protected const string SEMICOLON = ";";
        protected const string STATIC = "static";
        
        private const string OPEN_BRACE = "{";
        private const string CLOSE_BRACE = "}";
        private const int TAB_SPACES_COUNT = 4;

        internal AccessModifier AccessModifier { get; set; }
        internal string Name { get; }
        protected bool IsStatic { get; }

        protected GeneratableBase(string name, AccessModifier accessModifier, bool isStatic)
        {
            Name = name;
            AccessModifier = accessModifier;
            IsStatic = isStatic;
        }

        public abstract string GenerateStringRepresentation();
        
        // TODO: Make this abstract, and clean it up
        public IEnumerable<string> GenerateStringRepresentationLines()
        {
            var x = GenerateStringRepresentation().Split(Environment.NewLine);
            string[] y = new string[x.Length - 1];
            Array.Copy(x, y, y.Length);
            return y;
        }

        private string Tab(int count)
        {
            StringBuilder tab = new();
            for (int i = 0; i < TAB_SPACES_COUNT * count; i++) tab.Append(SPACE);
            return tab.ToString();
        }
        
        protected void AddLine(StringBuilder sb, int indent, string line) => sb.AppendLine(Tab(indent) + line);
        
        protected void AddLines(StringBuilder sb, int indent, IEnumerable<string> lines)
        {
            foreach (string line in lines) AddLine(sb, indent, line);
        }

        protected void AddEmptyLine(StringBuilder sb) => sb.AppendLine();

        protected void AddOpenBrace(StringBuilder sb, int indent) => AddLine(sb, indent, OPEN_BRACE);

        protected void AddCloseBrace(StringBuilder sb, int indent) => AddLine(sb, indent, CLOSE_BRACE);
        
        protected string GetTypeName(Type type)
        {
            if (type == typeof(string))	
                return "string";
            if (type == typeof(int))
                return "int";
            if (type == typeof(bool))	
                return "bool";
            if (type == typeof(byte))	
                return "byte";
            if (type == typeof(sbyte))	
                return "sbyte";
            if (type == typeof(char))	
                return "char";
            if (type == typeof(decimal))	
                return "decimal";
            if (type == typeof(double))	
                return "double";
            if (type == typeof(float))	
                return "float";
            if (type == typeof(int))	
                return "int";
            if (type == typeof(uint))	
                return "uint";
            if (type == typeof(nint))	
                return "nint";
            if (type == typeof(nuint))	
                return "nuint";
            if (type == typeof(long))	
                return "long";
            if (type == typeof(ulong))	
                return "ulong";
            if (type == typeof(short))	
                return "short";
            if (type == typeof(ushort))
                return "ushort";

            return type.Name;
        }
    }
}