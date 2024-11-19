# Input System Wrapper
## Changelog

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