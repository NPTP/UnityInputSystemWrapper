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
        private const string BINDING_INFOS = "IEnumerable<BindingInfo>";

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
                .WithDirectives("System", "System.Collections.Generic", GeneratedNamespaces.BINDINGS, GeneratedNamespaces.ENUMS)
                .WithMethod(SourceGen.NewMethod("StartInteractiveRebind")
                    .Public()
                    .ReturningVoid()
                    .Extending(ACTION_WRAPPER, "actionWrapper")
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of(REBIND_CALLBACK, "callback", "null"))
                    .Expression("actionWrapper.StartInteractiveRebind(controlScheme.ToId(), callback)"))
                .WithMethod(SourceGen.NewMethod("StartInteractiveRebind")
                    .Public()
                    .ReturningVoid()
                    .Extending(ACTION_WRAPPER, "actionWrapper")
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of("CompositePart", "compositePart"),
                        GeneratableParameter.Of(REBIND_CALLBACK, "callback", "null"))
                    .Expression("actionWrapper.StartInteractiveRebind(controlScheme.ToId(), compositePart, callback)"))
                .WithMethod(SourceGen.NewMethod("TryGetBindingInfo")
                    .Public()
                    .Returning<bool>()
                    .Extending(ACTION_WRAPPER, "actionWrapper")
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Out(BINDING_INFOS, "bindingInfos"))
                    .Expression("actionWrapper.TryGetBindingInfo(controlScheme.ToId(), out bindingInfos)"))
                .WithMethod(SourceGen.NewMethod("TryGetBindingInfo")
                    .Public()
                    .Returning<bool>()
                    .Extending(ACTION_WRAPPER, "actionWrapper")
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of("CompositePart", "compositePart"),
                        GeneratableParameter.Out(BINDING_INFOS, "bindingInfos"))
                    .Expression("actionWrapper.TryGetBindingInfo(controlScheme.ToId(), compositePart, out bindingInfos)"))
                .WithMethod(SourceGen.NewMethod("StartInteractiveRebind")
                    .Public()
                    .ReturningVoid()
                    .Extending(ACTION_REFERENCE, "actionReference")
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of(REBIND_CALLBACK, "callback", "null"))
                    .Expression("actionReference.StartInteractiveRebind(controlScheme.ToId(), callback)"))
                .WithMethod(SourceGen.NewMethod("TryGetBindingInfo")
                    .Public()
                    .Returning<bool>()
                    .Extending(ACTION_REFERENCE, "actionReference")
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Out(BINDING_INFOS, "bindingInfos"))
                    .Expression("actionReference.TryGetBindingInfo(controlScheme.ToId(), out bindingInfos)"));
        }
    }
}
