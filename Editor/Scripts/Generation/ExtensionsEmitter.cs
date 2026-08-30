using System.Collections.Generic;
using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Generatable;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits the enum-typed overloads of package APIs that would otherwise deal in the id structs. These
    /// have to be extensions, because the enums are generated and the types they extend are not.
    /// <para>
    /// Each extension class goes in the namespace of the type it extends, so a using directive the user
    /// already has brings the methods into scope. A player's own enum-typed surface is not here: it lives
    /// on <see cref="InputPlayerRefEmitter">InputPlayerRef</see> instead.
    /// </para>
    /// </summary>
    internal static class ExtensionsEmitter
    {
        private const string CONTROL_SCHEME = "ControlScheme";
        private const string ACTION_WRAPPER = "ActionWrapper";
        private const string ACTION_REFERENCE = "ActionReference";
        private const string REBIND_CALLBACK = "Action<RebindInfo>";
        private const string BINDING_SLOTS = "BindingSlots";
        private const string UI_INDEX = "uiIndex";

        internal static GeneratableFile BuildFile()
        {
            return SourceGen.NewFile()
                .WithHeaderComment(Helper.GetGeneratorNoticeLines().ToArray())
                .Containing(BuildUserChangeExtensions(), BuildActionExtensions());
        }

        private static GeneratableTypeDefinition BuildUserChangeExtensions()
        {
            return SourceGen.NewClass("InputUserChangeInfoExtensions").Public().Static()
                .InNamespace(GeneratedNamespaces.PLAYER)
                .WithDirectives(GeneratedNamespaces.ENUMS)
                .WithMethod(SourceGen.NewMethod(CONTROL_SCHEME)
                    .Public()
                    .Returning(CONTROL_SCHEME)
                    .Extending("InputUserChangeInfo", "info")
                    .Expression($"({CONTROL_SCHEME})info.ControlSchemeId.Index"));
        }

        /// <summary>
        /// The enum-typed overloads of the rebinding and binding-info API. These are genuine overloads
        /// sharing a name, told apart by their parameters.
        /// </summary>
        private static GeneratableTypeDefinition BuildActionExtensions()
        {
            return SourceGen.NewClass("ActionWrapperExtensions").Public().Static()
                .InNamespace(GeneratedNamespaces.ACTIONS)
                .WithDirectives("System", GeneratedNamespaces.BINDINGS, GeneratedNamespaces.ENUMS)
                .WithMethods(BuildRebindMethods(ACTION_WRAPPER, "actionWrapper", withCompositePart: true))
                .WithMethods(BuildRebindMethods(ACTION_REFERENCE, "actionReference", withCompositePart: false))
                .WithMethod(BuildGetSlotsMethod(ACTION_WRAPPER, "actionWrapper"))
                .WithMethod(BuildGetSlotsMethod(ACTION_REFERENCE, "actionReference"));
        }

        /// <summary>
        /// The rebind overloads for one type. The UI index says which of the action's slots on that control
        /// scheme to rebind, and defaults to the first, so a screen with one binding per action can leave it
        /// out entirely.
        /// <para>
        /// Only a type that does not already know its composite part gets the overload taking one. An
        /// ActionReference carries its part as a serialized field, so passing another one would be a second
        /// source of truth.
        /// </para>
        /// </summary>
        private static GeneratableMethod[] BuildRebindMethods(string extendedType, string parameterName, bool withCompositePart)
        {
            List<GeneratableMethod> rebindMethods = new()
            {
                SourceGen.NewMethod("StartInteractiveRebind")
                    .Public()
                    .ReturningVoid()
                    .Extending(extendedType, parameterName)
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of<int>(UI_INDEX, "0"),
                        GeneratableParameter.Of(REBIND_CALLBACK, "callback", "null"))
                    .Expression($"{parameterName}.StartInteractiveRebind(controlScheme.ToId(), {UI_INDEX}, callback)")
            };

            if (withCompositePart)
            {
                rebindMethods.Add(SourceGen.NewMethod("StartInteractiveRebind")
                    .Public()
                    .ReturningVoid()
                    .Extending(extendedType, parameterName)
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of("CompositePart", "compositePart"),
                        GeneratableParameter.Of<int>(UI_INDEX, "0"),
                        GeneratableParameter.Of(REBIND_CALLBACK, "callback", "null"))
                    .Expression($"{parameterName}.StartInteractiveRebind(controlScheme.ToId(), compositePart, {UI_INDEX}, callback)"));
            }

            return rebindMethods.ToArray();
        }

        private static GeneratableMethod BuildGetSlotsMethod(string extendedType, string parameterName)
        {
            return SourceGen.NewMethod("GetBindingSlots")
                .Public()
                .Returning(BINDING_SLOTS)
                .Extending(extendedType, parameterName)
                .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"))
                .Expression($"{parameterName}.GetBindingSlots(controlScheme.ToId())");
        }
    }
}
