# Input System Wrapper
## Changelog

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