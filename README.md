# Unity Input System Wrapper
### Nick Perrin (c) 2024

A wrapper for Unity's "new" input system to make usage simpler and more convenient with a foolproof and more readable API.

#### Advantages
- Subscribe to any player's input events at any time in Awake or later without running into race conditions or null references.
- Prevents double subscriptions to any input events.
- API is auto-generated by changes to actions/settings such that input events & polling are accessible in the most quick, easily-readable, boilerplate-free way.
- Switching devices on one player, joining new players on new devices, handling keyboard text input, rebinding controls, updating UI text & sprites for changing controls/devices, and more, are already handled for you requiring only simple subscriptions to the API.
- All input actions are centralized for a single source of truth for all input, even when using input action references (via the new InputActionReferenceWrapper) in the inspector, so that you know exactly where your input is coming from across your whole project, and can predict how it will behave when enabled/disabled/subscribed/polled/rebound.
- Dealing with a jungle of input maps turned on and off at any given time is no longer a worry with the new Input Context concept that collects maps in groups. Input Contexts can be added, changed & removed quickly in the settings, which then become immediately available in your code. 
- Easy component and API usage to handle rebinding controls for multiple devices.

#### Features to-do list:
- [✔] Change "Input Context" dynamically by a simple enum. No public access to particular action maps.
- [✔] Double subscription to a particular Input Action is rendered impossible.
- [✔] Subscribe to & unsubscribe from a player’s actions by ID at any time during play. Don’t need to know whether the player exists or not. In SP games, don’t even need the ID.
- [✔] In SP, event system is automatically set up for all. In MP, each player can control their own event systems.
- [✔] A player can be removed and their device returned to the pool of available devices for other players to use.
- [✔] In SP or MP games, when there’s only one player, they can swap to any device seamlessly.
- [✔] Event system actions get set for each player by their own local actions asset.
- [✔] A separate assembly so that some methods can be made internal and never callable by the outside game code.
- [✔] A "join mode" can be enabled where any button press on a currently unused device causes that device to be assigned to a new player.
- [✔] Remove Addressables dependencies
- [✔] Remove Unity Localization dependencies (for now)
- [✔] When a player swaps device, an event is invoked that sends the new device or control scheme and the player ID. In SP games, just the device is sent, no ID. (This can be used for updating UI elements)
- [✔] An input action reference can be translated to a binding on the currently active device, and that binding can be translated to a sprite for use on the UI. See: https://discussions.unity.com/t/get-action-bindings-for-specific-device/860105/2
- [✔] Support reporting multiple binding infos for each action given, since an action may have multiple bindings on a single device.
- [✔] Clear static members of the Input static class when domain reload is disabled in editor.
- [✔] Combine Actions and MapCache classes into one.
- [✔] Support wrapped input action properties and methods like polling input action state directly.
- [✔] Controls be rebound, with a simple button press to rebind. Currently support only 1 binding per action per device.
- [✔] Device->Binding combos are written to/read from disk. Rebind(InputAction Name, Binding). Input actions asset SaveBindingOverridesAsJson & LoadBindingOverridesFromJson. Saves binding overrides.
- [✔] Reset all bindings for a particular device.
- [ ] Sort out cases like composite bindings turned into a single binding button (e.g. "Move" binding, wiggle the joystick, maps to stick), and composites turned into multiple buttons (e.g. "Forward", "Left", "Down", "Right" for WASD on keyboard)
- [ ] General cleanup to make as many methods internal as possible
- [ ] Support control schemes: string control schemes get turned into enum as usual, but then runtime input data auto-generates a list of every control scheme with a slot to plug in binding data. Missing data fails silently. This will allow quick definition of of schemes in any game without needing manual device support like we currently have.

##### Multiplayer Additions:
- [ ] Input Action Reference -> BindingInfo can be player-dependent (the component for UI elements that uses this path can also reference a specific player), since players may each have unique bindings saved. By extension: Input.TryGetActionBindingInfo must support MP.

##### With Addressables/Localization Dependencies:
- [ ] Binding icon data only gets loaded in when needed instead of being always loaded
- [ ] Display name in BindingInfo is a LocalizedString, or has an event requesting a localized string that can be hooked into
