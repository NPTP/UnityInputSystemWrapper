using NPTP.InputSystemWrapper.Data;
using NPTP.InputSystemWrapper.Enums.NPTP.InputSystemWrapper;
using NPTP.UnitySourceGen.Editor;
using NPTP.UnitySourceGen.Editor.Generatable;
using UnityEngine.InputSystem;

namespace NPTP.InputSystemWrapper.Editor.Generation
{
    /// <summary>
    /// Emits the ISW facade: the public, enum-typed entry point users call, forwarding to the package's
    /// InputRuntime. It also wires the factory that lets InputPlayer build its generated actions objects.
    /// </summary>
    internal static class ISWEmitter
    {
        private const string CONTROL_SCHEME = "ControlScheme";
        private const string INPUT_CONTEXT = "InputContext";
        private const string INPUT_PLAYER = "InputPlayer";
        private const string ACTION_WRAPPER = "ActionWrapper";
        private const string PLAYER_ID = "playerID";
        private const string NULLABLE_PLAYER_ID = "int?";

        internal static GeneratableTypeDefinition Build(InputActionAsset asset, OfflineInputData offlineInputData)
        {
            GeneratableTypeDefinition isw = SourceGen.NewClass("ISW").Public().Static()
                .InNamespace(GeneratedNamespaces.ROOT)
                .WithDirectives("System", "System.Collections.Generic", "UnityEngine", "UnityEngine.InputSystem",
                    GeneratedNamespaces.ACTIONS, GeneratedNamespaces.ANY_BUTTON_PRESS, GeneratedNamespaces.BINDINGS,
                    GeneratedNamespaces.ENUMS, GeneratedNamespaces.PLAYER, GeneratedNamespaces.UTILITIES)
                .WithProperty(SourceGen.NewProperty("Runtime", "InputRuntime").Private().Static().Expression("InputRuntime.Current"))
                .WithProperty(SourceGen.NewProperty("DefaultPlayer", INPUT_PLAYER).Private().Static().Expression("Runtime.DefaultPlayer"));

            AddSinglePlayerAccess(isw, asset);
            AddInitialization(isw, asset, offlineInputData);
            AddEvents(isw);
            AddPublicInterface(isw);

            return isw;
        }

        /// <summary>
        /// The convenience surface for single-player games: the default player's actions, reachable without
        /// naming a player at all.
        /// </summary>
        private static void AddSinglePlayerAccess(GeneratableTypeDefinition isw, InputActionAsset asset)
        {
            foreach (string mapName in Helper.GetMapNames(asset))
            {
                isw.WithProperty(SourceGen.NewProperty(mapName.AsType(), $"{mapName.AsType()}Actions")
                    .Public()
                    .Static()
                    .Expression($"DefaultPlayer.{mapName.AsType()}()"));
            }

            isw.WithProperty(SourceGen.NewProperty("CurrentControlScheme", CONTROL_SCHEME)
                .Public()
                .Static()
                .Expression("DefaultPlayer.CurrentControlScheme()"));
        }

        private static void AddInitialization(GeneratableTypeDefinition isw, InputActionAsset asset, OfflineInputData offlineInputData)
        {
            GeneratableMethod initialize = SourceGen.NewMethod("Initialize")
                .Static()
                .ReturningVoid()
                .Body("InputPlayer.ActionMapWrapperFactory = CreateActionMapWrappers;",
                    "InputRuntime.Initialize();");

            if (offlineInputData.InitializationMode == InitializationMode.BeforeSceneLoad)
            {
                initialize.WithAttribute("RuntimeInitializeOnLoadMethod", "RuntimeInitializeLoadType.BeforeSceneLoad");
            }

            // Manual initialization is the user's to call, so it has to be reachable.
            if (offlineInputData.InitializationMode == InitializationMode.Manual) initialize.Public();
            else initialize.Private();

            isw.WithMethod(initialize);

            GeneratableMethod factory = SourceGen.NewMethod("CreateActionMapWrappers")
                .Private()
                .Static()
                .ReturningVoid()
                .Taking(GeneratableParameter.Of(INPUT_PLAYER, "player"),
                    GeneratableParameter.Of("Dictionary<string, IActionMapWrapper>", "wrappers"));

            string[] registrations = new string[asset.actionMaps.Count];
            for (int i = 0; i < asset.actionMaps.Count; i++)
            {
                string mapName = asset.actionMaps[i].name;
                registrations[i] = $"wrappers.Add(\"{mapName}\", new {mapName.AsType()}Actions(player.ID, player.Asset, player.ActionWrapperTable));";
            }

            isw.WithMethod(factory.Body(registrations));
        }

        /// <summary>
        /// Every event simply re-exposes the runtime's, so the accessors forward rather than storing any
        /// delegate of their own.
        /// </summary>
        private static void AddEvents(GeneratableTypeDefinition isw)
        {
            AddForwardedEvent(isw, "OnLocalizedStringRequested", "Action<LocalizedStringRequest>");
            AddForwardedEvent(isw, "OnControlsUpdated", "Action");
            AddForwardedEvent(isw, "OnAnyButtonPress", "AnyButtonPressListener");
            AddForwardedEvent(isw, "OnBindingsChanged", "Action");
            AddForwardedEvent(isw, "OnAnyPlayerInputUserChange", "Action<InputUserChangeInfo>");
            AddForwardedEvent(isw, "OnAnyPlayerControlSchemeChanged", $"Action<{INPUT_PLAYER}>");
            AddForwardedEvent(isw, "OnAnyPlayerKeyboardTextInput", "Action<char>");
        }

        private static void AddForwardedEvent(GeneratableTypeDefinition isw, string eventName, string handlerType)
        {
            isw.WithEvent(SourceGen.NewEvent(eventName)
                .Public()
                .Static()
                .Of(handlerType)
                .Forwarding($"Runtime.{eventName}"));
        }

        private static void AddPublicInterface(GeneratableTypeDefinition isw)
        {
            isw
                .WithProperty(SourceGen.NewProperty("MousePosition", "Vector2").Public().Static().Expression("Mouse.current.position.ReadValue()"))
                .WithMethod(SourceGen.NewMethod("GetPlayer").Public().Static().Returning(INPUT_PLAYER)
                    .Taking(GeneratableParameter.Of<int>(PLAYER_ID))
                    .Expression($"Runtime.GetPlayer({PLAYER_ID})"))
                .WithMethod(SourceGen.NewMethod("AddPlayer").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of<int>(PLAYER_ID))
                    .Expression($"Runtime.AddPlayer({PLAYER_ID})"))
                .WithMethod(SourceGen.NewMethod("RemovePlayer").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of<int>(PLAYER_ID))
                    .Expression($"Runtime.RemovePlayer({PLAYER_ID})"))
                .WithMethod(SourceGen.NewMethod("ControlSchemeHas").Public().Static().Returning<bool>()
                    .Generic(GeneratableTypeParameter.Of("TDevice", "InputDevice"))
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of<int>(PLAYER_ID, "0"))
                    .Expression($"Runtime.ControlSchemeHas<TDevice>(controlScheme.ToId(), {PLAYER_ID})"))
                .WithMethod(SourceGen.NewMethod("SetContextForAllPlayers").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of(INPUT_CONTEXT, "inputContext"))
                    .Expression("Runtime.SetContextForAllPlayers(inputContext.ToId())"))
                .WithMethod(SourceGen.NewMethod("TryConvert").Public().Static().Returning<bool>()
                    .Taking(GeneratableParameter.Of("InputActionReference", "inputActionReference"),
                        GeneratableParameter.Of<int>(PLAYER_ID),
                        GeneratableParameter.Out(ACTION_WRAPPER, "actionWrapper"))
                    .Expression($"Runtime.TryConvert(inputActionReference, {PLAYER_ID}, out actionWrapper)"))
                .WithMethod(SourceGen.NewMethod("TryConvert").Public().Static().Returning<bool>()
                    .Taking(GeneratableParameter.Of("InputActionReference", "inputActionReference"),
                        GeneratableParameter.Out(ACTION_WRAPPER, "actionWrapper"))
                    .Expression("Runtime.TryConvert(inputActionReference, out actionWrapper)"))
                .WithMethod(SourceGen.NewMethod("ResetBindingForAction").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of("ActionReference", "actionReference"),
                        GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"))
                    .Expression("Runtime.ResetBindingForAction(actionReference, controlScheme.ToId())"))
                .WithMethod(SourceGen.NewMethod("ResetAllBindingsForControlScheme").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of(CONTROL_SCHEME, "controlScheme"),
                        GeneratableParameter.Of(NULLABLE_PLAYER_ID, PLAYER_ID, "null"))
                    .Expression($"Runtime.ResetAllBindingsForControlScheme(controlScheme.ToId(), {PLAYER_ID})"))
                .WithMethod(SourceGen.NewMethod("LoadAllBindings").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of(NULLABLE_PLAYER_ID, PLAYER_ID, "null"))
                    .Expression($"Runtime.LoadAllBindings({PLAYER_ID})"))
                .WithMethod(SourceGen.NewMethod("SaveAllBindings").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of(NULLABLE_PLAYER_ID, PLAYER_ID, "null"))
                    .Expression($"Runtime.SaveAllBindings({PLAYER_ID})"))
                .WithMethod(SourceGen.NewMethod("ResetAllBindings").Public().Static().ReturningVoid()
                    .Taking(GeneratableParameter.Of(NULLABLE_PLAYER_ID, PLAYER_ID, "0"))
                    .Expression($"Runtime.ResetAllBindings({PLAYER_ID})"));

            isw.WithProperty(SourceGen.NewProperty<bool>("AllowPlayerJoining")
                .Public()
                .Static()
                .WithAccessors("Runtime.AllowPlayerJoining", "Runtime.AllowPlayerJoining = value"));
        }
    }
}
