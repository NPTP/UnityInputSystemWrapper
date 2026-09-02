using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Generatable;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits InputPlayerRef, the handle users hold onto a player. It exists because the generated actions
    /// classes cannot be members of InputPlayer itself - InputPlayer lives in the package, the generated
    /// types do not, and a partial class cannot span two assemblies. Wrapping it here lets the actions be
    /// properties rather than extension methods, so a player's actions read as
    /// ISW.GetPlayer(4).Gameplay.Fire.
    /// <para>
    /// It is a readonly struct holding nothing but the player, so passing one around costs no allocation,
    /// and it converts implicitly to and from InputPlayer so package APIs on either side keep working.
    /// </para>
    /// </summary>
    internal static class InputPlayerRefEmitter
    {
        internal const string TYPE_NAME = "InputPlayerRef";

        private const string CONTROL_SCHEME = "ControlScheme";
        private const string INPUT_CONTEXT = "InputContext";
        private const string INPUT_PLAYER = "InputPlayer";
        private const string FIELD = "inputPlayer";

        internal static GeneratableTypeDefinition Build(InputActionAsset asset)
        {
            GeneratableTypeDefinition playerRef = SourceGen.NewStruct(TYPE_NAME).Public().ReadOnly()
                .InNamespace(GeneratedNamespaces.PLAYER)
                .WithDirectives("System", GeneratedNamespaces.ACTIONS, GeneratedNamespaces.ANY_BUTTON_PRESS,
                    GeneratedNamespaces.ENUMS, "UnityEngine", "UnityEngine.InputSystem", "UnityEngine.InputSystem.UI",
                    "UnityEngine.UI")
                .WithField(SourceGen.NewField(FIELD, INPUT_PLAYER).Private().ReadOnly())
                .WithMethod(SourceGen.NewMethod(TYPE_NAME).Private().AsConstructor()
                    .Taking(GeneratableParameter.Of(INPUT_PLAYER, FIELD))
                    .Body($"this.{FIELD} = {FIELD};"));

            AddActions(playerRef, asset);
            AddState(playerRef);
            AddVirtualMouse(playerRef);
            AddEvents(playerRef);
            AddConversions(playerRef);

            return playerRef;
        }

        /// <summary>
        /// One property per action map. Each is the dictionary lookup InputPlayer does internally, so
        /// holding onto the result is no different from reading it each time.
        /// </summary>
        private static void AddActions(GeneratableTypeDefinition playerRef, InputActionAsset asset)
        {
            foreach (string mapName in ISWEditorHelper.GetMapNames(asset))
            {
                string actionsType = $"{mapName.AsType()}Actions";
                playerRef.WithProperty(SourceGen.NewProperty(mapName.AsType(), actionsType)
                    .Public()
                    .Expression($"({actionsType}){FIELD}.GetActionMapWrapper(\"{mapName}\")"));
            }
        }

        /// <summary>
        /// The player's own state, with the id structs the package uses internally presented as the
        /// generated enums instead.
        /// </summary>
        private static void AddState(GeneratableTypeDefinition playerRef)
        {
            playerRef
                .WithProperty(SourceGen.NewProperty<int>("ID").Public().Expression($"{FIELD}.ID"))
                .WithProperty(SourceGen.NewProperty<bool>("Enabled").Public()
                    .WithAccessors($"{FIELD}.Enabled", $"{FIELD}.Enabled = value"))
                .WithProperty(SourceGen.NewProperty("CurrentControlScheme", CONTROL_SCHEME).Public()
                    .Expression($"({CONTROL_SCHEME}){FIELD}.CurrentControlSchemeId.Index"))
                .WithProperty(SourceGen.NewProperty("InputContext", INPUT_CONTEXT).Public()
                    .WithAccessors($"({INPUT_CONTEXT}){FIELD}.InputContextId.Index", $"{FIELD}.InputContextId = value.ToId()"))
                .WithMethod(SourceGen.NewMethod("ControlSchemeHas").Public().Returning<bool>()
                    .Generic(GeneratableTypeParameter.Of("TDevice", "InputDevice"))
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"))
                    .Expression($"{FIELD}.ControlSchemeHas<TDevice>(controlScheme.ToId())"));
        }

        /// <summary>
        /// The mouse this player drives with the virtual mouse map's actions, for pointing at a UI with a
        /// gamepad.
        /// </summary>
        private static void AddVirtualMouse(GeneratableTypeDefinition playerRef)
        {
            playerRef
                .WithProperty(SourceGen.NewProperty<bool>("VirtualMouseEnabled").Public()
                    .Expression($"{FIELD}.VirtualMouseEnabled"))
                .WithMethod(SourceGen.NewMethod("EnableVirtualMouse").Public().ReturningVoid()
                    .Taking(GeneratableParameter.Of("RectTransform", "cursorTransform", "null"),
                        GeneratableParameter.Of("Graphic", "cursorGraphic", "null"),
                        GeneratableParameter.Of("VirtualMouseInput.CursorMode", "cursorMode",
                            "VirtualMouseInput.CursorMode.SoftwareCursor"))
                    .Expression($"{FIELD}.EnableVirtualMouse(cursorTransform, cursorGraphic, cursorMode)"))
                .WithMethod(SourceGen.NewMethod("DisableVirtualMouse").Public().ReturningVoid()
                    .Expression($"{FIELD}.DisableVirtualMouse()"));
        }

        /// <summary>
        /// The player's events, forwarded rather than stored, so subscribing through a handle and
        /// unsubscribing through another one still pairs up.
        /// </summary>
        private static void AddEvents(GeneratableTypeDefinition playerRef)
        {
            AddForwardedEvent(playerRef, "OnInputUserChange", "Action<InputUserChangeInfo>");
            AddForwardedEvent(playerRef, "OnControlSchemeChanged", $"Action<{INPUT_PLAYER}>");
            AddForwardedEvent(playerRef, "OnEnabledOrDisabled", $"Action<{INPUT_PLAYER}>");
            AddForwardedEvent(playerRef, "OnKeyboardTextInput", "Action<char>");
            AddForwardedEvent(playerRef, "OnAnyButtonPress", "AnyButtonPressListener");
        }

        private static void AddForwardedEvent(GeneratableTypeDefinition playerRef, string eventName, string handlerType)
        {
            playerRef.WithEvent(SourceGen.NewEvent(eventName)
                .Public()
                .OfType(handlerType)
                .Forwarding($"{FIELD}.{eventName}"));
        }

        /// <summary>
        /// Implicit both ways, so a player handed back by a package event can be used as a handle without
        /// a cast, and a handle can be passed to anything still taking an InputPlayer.
        /// </summary>
        private static void AddConversions(GeneratableTypeDefinition playerRef)
        {
            playerRef
                .WithMethod(SourceGen.NewMethod(TYPE_NAME).AsImplicitConversion()
                    .Taking(GeneratableParameter.Of(INPUT_PLAYER, FIELD))
                    .Expression($"new {TYPE_NAME}({FIELD})"))
                .WithMethod(SourceGen.NewMethod(INPUT_PLAYER).AsImplicitConversion()
                    .Taking(GeneratableParameter.Of(TYPE_NAME, "playerRef"))
                    .Expression($"playerRef.{FIELD}"));
        }
    }
}
