# Unity Input System Wrapper
### Nick Perrin (c) 2024

A wrapper for Unity's "new" input system to make usage simpler and more convenient with a foolproof and more readable API.

#### Advantages
- Subscribe to any player's input events at any time in Awake or later without running into race conditions or null references. Don’t need to know whether the player exists or not. In singleplayer mode, the API is automatically simplified so you don’t even need to specify which player.
- Prevents double subscriptions to any input events.
- Easy player join mode for joining new devices and pairing to under-the-hood input players.
- API is auto-generated by changes to actions/settings such that input events & polling are accessible in the most quick, easily-readable, boilerplate-free (and string-literal-free!) way. Only the methods & properties needed for your game are exposed from the internal assembly to keep things simple. Any changes to your input action asset will be immediately reflected in the generated API so you can fix any broken usage immediately without hunting for broken references or out-of-date strings.
- Switching devices on one player, joining new players on new devices, handling keyboard text input, rebinding controls, updating UI text & sprites for changing controls/devices, and more, are already handled for you requiring only simple subscriptions to the API, with direct access checks also available if you don't want to or can't use the events.
- All input actions are centralized for a single source of truth for all input, even when using input action references (via the new ActionReference) in the inspector, so that you know exactly where your input is coming from across your whole project, and can predict how it will behave when enabled/disabled/subscribed/polled/rebound.
- Dealing with a jungle of input maps turned on and off at any given time is no longer a worry with the new InputContext concept that collects maps in groups. InputContexts can be added, changed & removed quickly in the settings, which then become immediately available in your code. You can even specify particular actions for the event system to use, per InputContext.
- Easy component and API usage to handle rebinding controls for multiple devices.

TODO's exist in the code for immediate next attention, as well as the following nice-to-haves.

#### Future nice-to-haves:
- [✔] Display name in BindingInfo has an event requesting a localized string that can be hooked into. Otherwise, we could use LocalizedString (which would require a Localization package dependency)
- [✔] Optimize Input Context switching
- [ ] Reformat all classes with code-gen into partial classes with partial method calls that separate generated from non-generated .cs files for easier modularity
- [ ] OfflineInputData "excluded" and "cancel" controls are selectable from non-duplicate-entry, foolproof dropdown of all possible paths instead of string fields. Format themselves correctly.
- [ ] Support multiple re-binds per action, per player. Let developer choose how many bindings an action is allowed to have per control scheme (runtime settings?).
- [ ] Binding icon data only gets loaded in when needed instead of being always loaded (Addressables package dependency, should work just like localization strings do with Addressables)
- [ ] Control schemes currently generate a field in RuntimeInputData for their BindingData, but that's all. Ideally, have them automatically generate a BindingData, if it does not already exist, that contains all of the control paths for the devices in the control scheme.
