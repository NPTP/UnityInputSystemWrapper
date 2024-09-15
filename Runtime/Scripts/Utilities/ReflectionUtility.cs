using System;
using System.Reflection;

namespace NPTP.InputSystemWrapper.Utilities
{
    internal static class ReflectionUtility
    {
        /// <summary>
        /// Sets static members of a static class to default values.
        /// For properties, only properties with a backing field will get reset.
        /// For events, only events with a backing field will get reset.
        /// This avoids potential issues with invoking complex logic inside property/event setters.
        /// Note that const and readonly fields will not be changed.
        /// </summary>
        public static void ResetStaticClassMembersToDefault(Type staticType)
        {
            if (!staticType.IsAbstract || !staticType.IsSealed)
                return;

            FieldInfo[] fields = staticType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        
            // Fields
            foreach (FieldInfo field in fields)
            {
                if (!field.IsLiteral && !field.IsInitOnly)
                {
                    object defaultValue = GetDefaultValue(field.FieldType);
                    field.SetValue(null, defaultValue);
                }
            }
            
            // Properties
            PropertyInfo[] properties = staticType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite)
                {
                    FieldInfo backingField = staticType.GetField(GetPropertyBackingFieldName(property), BindingFlags.NonPublic | BindingFlags.Static);
                    if (backingField != null)
                    {
                        backingField.SetValue(null, GetDefaultValue(backingField.FieldType));
                    }
                }
            }

            // Events
            EventInfo[] events = staticType.GetEvents(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (EventInfo eventInfo in events)
            {
                FieldInfo eventField = staticType.GetField(eventInfo.Name, BindingFlags.Static | BindingFlags.NonPublic);
                if (eventField != null)
                {
                    eventField.SetValue(null, null);
                }
            }
        }
        
        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
        
        private static string GetPropertyBackingFieldName(PropertyInfo propertyInfo)
        { 
            return $"<{propertyInfo.Name}>k__BackingField";
        }
    }
}