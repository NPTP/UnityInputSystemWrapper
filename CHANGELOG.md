# Input System Wrapper
## Changelog

10.0.0
- The composite part on an ActionReference offers only the parts the action can have: an action read as a float comes from an axis composite, so positive and negative, while a Vector2 action's composite has up, down, left and right
- `ActionReference.PlayerID` is public and settable, so one screen can be pointed at each player in turn instead of needing a reference per player. Setting it drops the cached action wrapper, which belongs to the player it pointed at before
- `ActionReference.applyToAllPlayers` is gone. It was serialized but never read by anything
- `ActionReference.useCompositePart` is gone. `CompositePart.DontIsolatePart` already meant the whole binding, so the bool only restated it
- `com.unity.addressables` is a required dependency, resolved automatically from Unity's registry
- Upgrading: `BindingInfo` is a ScriptableObject rather than a struct, so `BindingSlot.BindingInfo` is a reference instead of a nullable. Existing binding data assets do not carry over and are rebuilt by a regenerate, which also marks everything addressable
- Binding data moved out of the generated Resources folder to `ISW.Generated/BindingData`, since a Resources folder ships everything in it and these are reached through Addressables. Existing assets are moved there on the next generation
- `InputBindingDisplay`, a component that shows one binding of an action: handle its events to fill a TextMeshPro label, a sprite renderer, a UI Image, or anything else. Its assets load in the background, so a screen full of glyphs opens without stalling the frame, and they are released when it is disabled
- `BindingDiagnostics` reports how many binding assets are loaded and how many references are outstanding, for checking that loads and disposals balance
- `GetCurrentBindingSlotsAsync` on `ActionReference` and `ActionWrapper` loads binding slots without blocking
- Binding data for a device no longer used by any control scheme is deleted, along with its entry assets and all of their addressable entries. An entry asset for a control a device no longer has goes the same way
- Each binding entry is its own addressable asset, in a folder named for the binding data asset it belongs to, so a screen loads only the bindings it shows. `BindingInfo` is a ScriptableObject rather than a struct
- Binding data is addressable and loads per device only when something asks to display that device's controls, instead of every device's data being resident because the input data asset references it
- Generation marks each binding data asset addressable in an "ISW Data Group" of its own, so nothing has to be set up by hand and the assets stay out of the project default group
- `BindingSlots` holds the binding data its slots were built from and implements IDisposable: dispose it when the screen showing it closes. A set dropped without being disposed releases its data when the garbage collector reaches it
- Binding data wanted by several slot sets is loaded once and released when the last of them lets go
- A binding entry has a default display name, shown when no localization request comes back fulfilled. `BindingInfo.DisplayName` used to fall back to the raw localization key
- Generated entries start with a localization key qualified by device, e.g. `Gamepad/leftStick/x`, and a display name parsed from the control path, e.g. "Left Stick Up", with "dpad" in any casing reading as "D-Pad". Both are editable, and a regenerate fills in blanks without touching anything already authored
- `InputActionUpdater` owns the slots it hands to its event and replaces them on each update, so its handler should read what it needs rather than holding on

9.0.1
- The generator notice is on every generated file, including the actions classes, `InputPlayerRef` and `ISW`, which were written without one
- The generated binding data menu items share one selection method instead of repeating the same lookup per asset

9.0.0
- `BindingSerializationMode` on the input data asset chooses where saved bindings live: a JSON file per player, the project's own storage through events, both, or neither
- `ISW.OnBindingsSaveRequested` hands out a player's bindings as JSON to store; `ISW.OnBindingsLoadRequested` asks for them back
- With both sources enabled, stored bindings supplied by the project take precedence over the file
- A control scheme's device family is derived from the devices it requires instead of being authored: there is a family per device layout the input system registers directly under InputDevice (pointer, gamepad, keyboard, joystick, sensor, tracked device), and a scheme requiring several is all of them. Layouts match by inheritance, so a mouse, pen or touchscreen all count as pointers and a DualShockGamepad counts as a gamepad. The Control Scheme Device Families section is gone, and `ControlScheme.IsMouseBased()` becomes `ControlScheme.UsesPointer()`, alongside `UsesGamepad`, `UsesKeyboard`, `UsesJoystick`, `UsesSensor` and `UsesTrackedDevice`
- `ActionReference<T>` reads values with the same `ReadValue()` syntax and type safety as the generated ISW properties, resolving to the `ValueActionWrapper<T>` the action already has
- Every `ActionReference` picks its action from a dropdown of the project input data's input action asset. On an `ActionReference<T>` only actions read as T are offered, so an assigned reference always has a value to read
- The default event system actions are picked from a dropdown of the assigned input action asset's actions, grouped by action map, rather than from any InputActionReference in the project. The event system action overrides on an input context use the same dropdown. The default actions section is replaced by a note when no asset is assigned

8.0.0
- An action can have any number of bindings per control scheme, each addressed by a UI index. Previously only the first was reachable
- A UI index names one slot: a plain binding, or a whole composite counted as one. A d-pad taking four entries in the input action asset stays on the row the player last saw it on
- `StartInteractiveRebind` takes a UI index, defaulting to the first slot. Rebinding a composite slot requires a composite part, and says so instead of rebinding the wrong control
- Reading bindings returns `BindingSlots`: enumerable, indexable, with `TryGetAtUIIndex` that warns with the indices an action actually has rather than throwing
- `ResetBindingForAction` resets one slot and requires a UI index; `ResetAllBindingsForAction` resets every binding the action has on a control scheme
- `ResetBinding`, `ResetAllBindings` and `GetBindingSlots` are generated as extensions on `ActionWrapper` and `ActionReference`, each type in its own extension class
- Generation reports bindings belonging to no control scheme, which fire their action but can appear on no rebinding screen
- Saved binding overrides whose binding no longer exists in the input action asset are skipped and reported once on load, instead of one input system warning each
- Upgrading: `TryGetBindingInfo` and `TryGetCurrentBindingInfo` are replaced by `GetBindingSlots` and `GetCurrentBindingSlots`. `RebindInfo.BindingInfos` becomes `RebindInfo.BindingSlots`, and `InputActionUpdater.OnBindingsUpdated` carries `BindingSlots`

7.0.0
- `ISW.GetPlayer` returns the generated `InputPlayerRef`, whose actions are properties, so a player's actions read as `ISW.GetPlayer(4).Gameplay.Fire.OnEvent += HandleEvent`
- `InputPlayerRef` also carries the player's id, enabled state, current control scheme, input context and events, and converts implicitly to and from `InputPlayer`
- The generated extension methods on `InputPlayer` are gone, replaced by those properties. The generated `InputPlayerExtensions.cs` becomes `Extensions.cs`, keeping the `ActionWrapper`, `ActionReference` and `InputUserChangeInfo` extensions
- The default player's actions stay available without an id, as `ISW.Gameplay`
- `ISW.AddPlayer` is gone, along with `InputRuntime.AddPlayer`. `GetPlayer` already adds a player that does not exist yet
- Upgrading: replace `player.Gameplay()` with `player.Gameplay`, and likewise for `UI()`, `CurrentControlScheme()`, `GetInputContext()` and `SetInputContext()`, which is now the `InputContext` property

6.0.0
- `OfflineInputData` and `RuntimeInputData` are merged into a single `InputData` asset, loaded from Resources under that name. Authored and generated fields live together, with the editor-only ones excluded from builds as before
- Values the runtime can use as authored are no longer copied between assets, so the value a designer edits is the one the game reads. Only InputActionReferences are still baked, into the action IDs a player's cloned asset can resolve
- One inspector for the whole asset, `InputDataEditor`, with a section for the input action asset and custom setups
- The `Input > Offline Input Data` and `Input > Runtime Input Data` menu items are replaced by `Input > Input Data`
- `InitializationMode` moved out of an accidentally nested namespace into `NPTP.InputSystemWrapper.Enums`
- Upgrading: the old two assets are not migrated. Assign your input action asset on the new `InputData` and re-author input contexts, control scheme bases and event system actions

5.1.0
- Binding data is keyed by device rather than by control scheme, so a device used by several schemes has one set of entries instead of a copy per scheme
- A `BindingData` asset is generated per device if one does not exist, populated from the input system's own layout registry with every control that device has and the display names it gives them
- The default `BindingData` assets no longer ship with the package, since they are generated more precisely than the ones that were provided
- `BindingData` inspector shows every entry expanded: control path and localization key on the left, a square sprite field on the right, with a search box. Control paths come from generation and are no longer editable

5.0.0
- Package can now be installed read-only, e.g. by git URL. No code is generated into the package any more
- Generated code lives in its own assembly in your project (`Assets/ISW.Generated` by default), which reaches package internals through `InternalsVisibleTo`
- Editable input assets are created in your project on first generation; the package's copies are defaults only
- Code generation is built on the UnitySourceGen package rather than text templates and markers
- Generation only rewrites files whose contents changed, and reports one console entry per run instead of one per file
- Stale generated scripts are deleted when an action map is renamed or removed
- Data that was generated as C# now lives on the `RuntimeInputData` asset: control scheme metadata, input contexts, event system options and rebinding paths
- `ISW` is a generated facade over the new `InputRuntime`, which is an instance rather than a static class
- `InputPlayer` is no longer partial. Its generated members moved to extension methods:
  - `player.UI` becomes `player.UI()`
  - `player.CurrentControlScheme` becomes `player.CurrentControlScheme()`
  - `player.InputContext` becomes `player.GetInputContext()` / `player.SetInputContext(x)`
  - `changeInfo.ControlScheme` becomes `changeInfo.ControlScheme()`
- `ISW.Player(id)` renamed to `ISW.GetPlayer(id)`
- `ControlScheme.None` is now `-1`, so enum values match the control scheme order in the input action asset. Re-check any serialized `ControlScheme` fields
- `ControlSchemeBasis` and the default context are keyed by name rather than by enum, so re-set them in `OfflineInputData`
- Generation warns when an input context names an action map that does not exist, or has no active maps
- New `Input > Binding Data` submenu with a shortcut per binding data asset
- Excluded and cancel rebinding paths in `OfflineInputData` are chosen from a searchable dropdown grouped by device, rather than typed as strings. Paths already in the list are not offered again

4.0.0
- Rename `Input` class to `ISW` (acronym) to avoid needing aliases against Unity's built-in "Input" class.
- Multiplayer support initial version working.
- `OnAnyButtonPress` event uses new custom delegate `AnyButtonPressListener` (same signature as before). This applies to all devices.
  - Individual players now have their own non-global `OnAnyButtonPress` event which applies only to devices paired with that player at the time of invocation.
- ActionWrapper events pass a custom struct now instead of Unity's `InputAction.CallbackContext`, for better encapsulation and cordoning-off of properties that were accessible in Unity's struct that could break the ISW architecture.
- Separated auto-generated code into partial classes in separate folder to make package updates simpler.

3.2.3
- Editor-only changes:
  - Use root path identifier serialized field instead of making user set script path
  - Clean up custom inspectors to hide fields that shouldn't be changed by user

3.2.2
- Numerous editor and runtime scripts changed from public classes/methods/fields to internal, public custom setup classes sealed
- Input.WaitForAnyButtonPress custom yield instruction is now reusable without creating a new instance

3.2.1
- Temporary, but useful, `Input.OnControlsUpdated` event added for catch-all UI updates

3.1.1
- Custom setups (bindings, layouts, interactions) are now asset-based and specified inside the RuntimeInputData asset for easier developer expansion

3.1.0
- Specify control schemes as either being mouse- or gamepad-based, higher abstraction
- Fix binding composite part name recognition
- Implement new custom input binding composite targeting UI navigation specifically

3.0.3
- Binding overrides save to persistent data path json file instead of PlayerPrefs for better management of player settings.
- Add option to specify whether binding overrides should be loaded on ISW initialization or not.
- Improved OfflineInputData editor window.

3.0.2
- Specify invariant culture for string generation on floats, fixes incorrect characters in some string cultures.
- Fix non-composite rebinding of value action (e.g. Left Stick binding for a Vector2-valued input action)
- Partial fix for rebinding cancellation with a cancel path that isn't in the player's current control scheme.

3.0.1
- Remove dysfunctional custom interaction, fixes a build bug.

3.0.0
- Interactive rebind callback now returns a struct with relevant rebind info.

2.0.5
- Improvements & fixes mostly in editor code.
- Prevent EDITOR_ prefix code from being accessed outside the package's assemblies.

2.0.4
- Fix bug with custom binding & device registrations not being found in builds

2.0.3
- Fix bug with missing ! negation that prevents binding info from being found

2.0.2
- Fix bug where leftover action map scripts in Runtime/Scripts/Generated/Actions wouldn't get cleared on changes to action maps

2.0.1
- Optimizations in ActionWrapper lookup
- Refactors to further centralize everything out of player-asset-situated ActionWrappers

2.0.0
- Architectural/API changes around rebinding controls
- BindingInfo uses event for hooking binding display names into any localization system via Input.OnLocalizedStringRequested.

1.1.1
- Input contexts now support event system action overrides.
- Debugger window expanded to help see top-level view of system at runtime during play.
- Event System global options specified in offline input data.

1.1.0
- Actions are now accessed via one more layer of wrapping. This makes more sense to read. E.g.: Input.UI.OnClick event becomes Input.UI.Click.OnEvent, to fit with other functionality inside Input.UI.Click.
- ActionWrapper now supports buttons, values and pass-throughs, with explicitly-typed ReadValue methods.

1.0.0
- Initial version