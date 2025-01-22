using System;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration.Enums
{
    public enum TypeDefinition
    {
        Class = 0,
        Struct,
        Record
    }

    public static class TypeDefinitionExtensions
    {
        public static string AsString(this TypeDefinition typeDefinition)
        {
            return typeDefinition switch
            {
                TypeDefinition.Class => "class",
                TypeDefinition.Struct => "struct",
                TypeDefinition.Record => "record",
                _ => throw new ArgumentOutOfRangeException(nameof(typeDefinition), typeDefinition, null)
            };
        }
    }
}