using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Enums;
using NPTP.UnitySourceGen.Editor.Generatable;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits the extension methods that give type-safe access to a player's generated actions and to the
    /// enum-typed views of its state. These cannot be members on InputPlayer itself, because InputPlayer
    /// lives in the package and the generated types do not.
    /// <para>
    /// Each extension class goes in the namespace of the type it extends, so a using directive the user
    /// already has brings the methods into scope.
    /// </para>
    /// </summary>
    internal static class InputPlayerExtensionsEmitter
    {
        private const string CONTROL_SCHEME = "ControlScheme";
        private const string INPUT_CONTEXT = "InputContext";
        private const string INPUT_PLAYER = "InputPlayer";
        private const string ACTION_WRAPPER = "ActionWrapper";
        private const string ACTION_REFERENCE = "ActionReference";
        private const string REBIND_CALLBACK = "Action<RebindInfo>";
        private const string BINDING_INFOS = "IEnumerable<BindingInfo>";

        internal static GeneratableFile BuildFile(InputActionAsset asset)
        {
            return SourceGen.NewFile()
                .WithHeaderComment(Helper.GetGeneratorNoticeLines().ToArray())
                .Containing(BuildPlayerExtensions(asset), BuildActionExtensions());
        }

        private static GeneratableTypeDefinition BuildPlayerExtensions(InputActionAsset asset)
        {
            GeneratableTypeDefinition playerExtensions = SourceGen.NewStaticClass("InputPlayerExtensions", AccessModifier.Public)
                .InNamespace(GeneratedNamespaces.PLAYER)
                .WithDirectives(GeneratedNamespaces.ROOT, GeneratedNamespaces.ACTIONS, GeneratedNamespaces.ENUMS, "UnityEngine.InputSystem");

            foreach (string mapName in Helper.GetMapNames(asset))
            {
                string actionsType = $"{mapName.AsType()}Actions";
                playerExtensions.WithMethod(SourceGen.NewMethod(mapName.AsType())
                    .Public()
                    .Returning(actionsType)
                    .Extending(INPUT_PLAYER, "inputPlayer")
                    .Expression($"({actionsType})inputPlayer.GetActionMapWrapper(\"{mapName}\")"));
            }

            return playerExtensions
                .WithMethod(SourceGen.NewMethod("CurrentControlScheme")
                    .Public()
                    .Returning(CONTROL_SCHEME)
                    .Extending(INPUT_PLAYER, "inputPlayer")
                    .Expression($"({CONTROL_SCHEME})inputPlayer.CurrentControlSchemeId.Index"))
                .WithMethod(SourceGen.NewMethod(CONTROL_SCHEME)
                    .Public()
                    .Returning(CONTROL_SCHEME)
                    .Extending("InputUserChangeInfo", "info")
                    .Expression($"({CONTROL_SCHEME})info.ControlSchemeId.Index"))
                .WithMethod(SourceGen.NewMethod("GetInputContext")
                    .Public()
                    .Returning(INPUT_CONTEXT)
                    .Extending(INPUT_PLAYER, "inputPlayer")
                    .Expression($"({INPUT_CONTEXT})inputPlayer.InputContextId.Index"))
                .WithMethod(SourceGen.NewMethod("SetInputContext")
                    .Public()
                    .ReturningVoid()
                    .Extending(INPUT_PLAYER, "inputPlayer")
                    .Taking(GeneratableParameter.Of(INPUT_CONTEXT, "inputContext"))
                    .Expression("inputPlayer.InputContextId = inputContext.ToId()"))
                .WithMethod(SourceGen.NewMethod("ControlSchemeHas")
                    .Public()
                    .Returning<bool>()
                    .Generic(GeneratableTypeParameter.Of("TDevice", "InputDevice"))
                    .Extending(INPUT_PLAYER, "inputPlayer")
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"))
                    .Expression("inputPlayer.ControlSchemeHas<TDevice>(controlScheme.ToId())"));
        }

        /// <summary>
        /// The enum-typed overloads of the rebinding and binding-info API. These are genuine overloads
        /// sharing a name, told apart by their parameters.
        /// </summary>
        private static GeneratableTypeDefinition BuildActionExtensions()
        {
            return SourceGen.NewStaticClass("ActionWrapperExtensions", AccessModifier.Public)
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
