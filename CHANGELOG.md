# Input System Wrapper
## Changelog

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