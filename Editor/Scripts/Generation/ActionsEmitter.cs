using NPTP.InputSystemWrapper.Actions;
using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Generatable;
using NPTP.UnitySourceGen.Editor.Syntax;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits one actions class per action map: a typed property per action, plus the enable/disable
    /// members the runtime calls through <see cref="IActionMapWrapper"/>.
    /// <para>
    /// The interface members are implemented explicitly, so they stay off the class's own public surface -
    /// users see only their actions, not the plumbing.
    /// </para>
    /// </summary>
    internal static class ActionsEmitter
    {
        private const string ACTION_MAP = "ActionMap";
        private const string ENABLED = "enabled";
        private const string TABLE_TYPE = "Dictionary<Guid, ActionWrapper>";

        internal static GeneratableTypeDefinition Build(InputActionMap map)
        {
            string className = $"{map.name.AsType()}Actions";

            GeneratableTypeDefinition actions = SourceGen.NewClass(className).Public()
                .WithInheritanceModifier(InheritanceModifier.Sealed)
                .ImplementsInterface("IActionMapWrapper")
                .InNamespace(GeneratedNamespaces.ACTIONS)
                .WithDirectives("System", "System.Collections.Generic", "UnityEngine", "UnityEngine.InputSystem",
                    "UnityEngine.InputSystem.XR", GeneratedNamespaces.ACTIONS)
                .WithProperty(SourceGen.NewProperty(ACTION_MAP, "InputActionMap").Internal().GetOnly())
                .WithField(SourceGen.NewField(ENABLED, "bool").Private());

            foreach (InputAction action in map)
            {
                TypeRef wrapperType = GetWrapperType(action);
                if (wrapperType.IsVoid)
                {
                    continue;
                }

                actions.WithProperty(SourceGen.NewProperty(action.name.AsProperty(), wrapperType).Public().GetOnly());
            }

            actions.WithMethod(BuildConstructor(map, className));
            actions.WithMethod(BuildCallbackToggle(map, "EnableAndRegisterCallbacks", enable: true));
            actions.WithMethod(BuildCallbackToggle(map, "DisableAndUnregisterCallbacks", enable: false));

            return actions;
        }

        private static GeneratableMethod BuildConstructor(InputActionMap map, string className)
        {
            GeneratableMethod constructor = SourceGen.NewMethod(className)
                .AsConstructor()
                .Internal()
                .Taking(GeneratableParameter.Of<int>("playerID"),
                    GeneratableParameter.Of("InputActionAsset", "asset"),
                    GeneratableParameter.Of(TABLE_TYPE, "table"));

            string[] body = new string[1 + CountWrappedActions(map)];
            body[0] = $"{ACTION_MAP} = asset.FindActionMap(\"{map.name}\", throwIfNotFound: true);";

            int i = 1;
            foreach (InputAction action in map)
            {
                if (GetWrapperType(action).IsVoid) continue;
                body[i++] = $"{action.name.AsProperty()} = new (playerID, {ACTION_MAP}.FindAction(\"{action.name}\", throwIfNotFound: true), table);";
            }

            return constructor.Body(body);
        }

        private static GeneratableMethod BuildCallbackToggle(InputActionMap map, string methodName, bool enable)
        {
            string guard = enable ? ENABLED : $"!{ENABLED}";
            string mapCall = enable ? $"{ACTION_MAP}.Enable();" : $"{ACTION_MAP}.Disable();";
            string registration = enable ? "RegisterCallbacks" : "UnregisterCallbacks";

            string[] header =
            {
                $"if ({guard})",
                "{",
                "    return;",
                "}",
                string.Empty,
                $"{ENABLED} = {(enable ? "true" : "false")};",
                mapCall,
                string.Empty
            };

            string[] body = new string[header.Length + CountWrappedActions(map)];
            header.CopyTo(body, 0);

            int i = header.Length;
            foreach (InputAction action in map)
            {
                if (GetWrapperType(action).IsVoid) continue;
                body[i++] = $"{action.name.AsProperty()}.{registration}();";
            }

            return SourceGen.NewMethod(methodName)
                .ExplicitlyImplementing("IActionMapWrapper")
                .ReturningVoid()
                .Body(body);
        }

        private static int CountWrappedActions(InputActionMap map)
        {
            int count = 0;
            foreach (InputAction action in map)
            {
                if (!GetWrapperType(action).IsVoid) count++;
            }

            return count;
        }

        /// <summary>
        /// Buttons get the plain wrapper; values and pass-throughs get a typed one, or the untyped
        /// AnyValueActionWrapper when the asset does not specify a control type.
        /// </summary>
        private static TypeRef GetWrapperType(InputAction action)
        {
            if (action.type is InputActionType.Button)
            {
                return nameof(ActionWrapper);
            }

            if (action.type is not (InputActionType.Value or InputActionType.PassThrough))
            {
                return TypeRef.Void;
            }

            string expectedControlType = action.expectedControlType;
            return string.IsNullOrEmpty(expectedControlType)
                ? nameof(AnyValueActionWrapper)
                : TypeRef.Generic(nameof(ValueActionWrapper), ControlTypeToTypeName(expectedControlType));
        }

        /// <summary>
        /// Converts Unity's InputAction.expectedControlType string into the C# type keyword for that value
        /// (e.g. "Integer" becomes "int").
        /// </summary>
        private static string ControlTypeToTypeName(string controlType)
        {
            return controlType switch
            {
                "Analog" => "float",
                "Axis" => "float",
                "Bone" => "Bone",
                "Button" => "float",
                "Delta" => "Vector2",
                "Digital" => "int",
                "DiscreteButton" => "int",
                "Double" => "double",
                "Dpad" => "Vector2",
                "Eyes" => "Eyes",
                "Integer" => "int",
                "Pose" => "Pose",
                "Quaternion" => "Quaternion",
                "Stick" => "Vector2",
                "Touch" => "float", // TODO (control types): Unknown
                "Vector2" => "Vector2",
                "Vector3" => "Vector3",
                _ => controlType
            };
        }
    }
}
