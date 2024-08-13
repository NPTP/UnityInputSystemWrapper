# Unity Input System Wrapper
### Nick Perrin (c) 2024

A wrapper for Unity's "new" input system to make usage simpler and more convenient with a foolproof and much more readable API.

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
- [ ] When a player swaps device, an event is invoked that sends the new device and the player ID. In SP games, just the device is sent, no ID. (This can be used for updating UI elements)
- [ ] An input action reference can be translated to a binding on the currently active device, and that binding can be translated to a sprite for use on the UI.
- [ ] Controls be rebound, with a simple button press to rebind. Device->Binding combos are written to/read from disk. Rebind(InputAction Name, Binding).
- [ ] Remove Addressables dependencies
- [ ] Remove Unity Localization dependencies (for now)
