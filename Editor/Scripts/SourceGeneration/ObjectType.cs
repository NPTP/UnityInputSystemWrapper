using System;

namespace NPTP.InputSystemWrapper.Editor.SourceGeneration
{
    public enum ObjectType
    {
        Class = 0,
        Struct,
        Record
    }

    public static class ObjectTypeExtensions
    {
        public static string AsString(this ObjectType objectType)
        {
            return objectType switch
            {
                ObjectType.Class => "class",
                ObjectType.Struct => "struct",
                ObjectType.Record => "record",
                _ => throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null)
            };
        }
    }
}