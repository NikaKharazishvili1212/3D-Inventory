# 3D Inventory System

A Unity 3D project built to explore how a fully three-dimensional inventory system
could work — where items are real physical objects you drag through the world,
not just icons in a UI panel.

The inventory chest lives inside the player camera, creating the illusion of a
fixed UI while keeping everything in 3D space. Items have weight, fall with
physics, and make different sounds depending on what they are.

## Controls & Gameplay
WASD — move
LMB drag — grab and move an item
LMB drag + Mouse Scroll — push or pull the item closer or further
LMB + RMB — rotate held item
E — take item into inventory
I — open/close chest
U (hold) — preview inventory icons
G — throw item / drop all from chest
Click item in chest — drop it back into the world

## Inventory Features
- Tracks total weight and item count against the slot limit
- Empty slots automatically sort and fill when items are removed
- Hold U to preview stored items as 2D icons
- Items spawn randomly into the scene up to a set limit
- Fake cursor replaces the system cursor for a seamless 3D feel

## Architecture Notes
- All input is handled exclusively in `Player.cs`
- `Item.cs` manages only its own physics state and inventory transitions
- `Inventory.cs` manages slots, weight, UI, and chest animation
- `GlobalReferences.cs` acts as a singleton hub — no cross-script Find calls anywhere
- Magic numbers are centralized in `GameConstants.cs`

## License
This repository is for viewing purposes only. All content, including third-party assets, may not be reused, copied, or redistributed without explicit permission.

## Watch the Demo on YouTube
[![Watch the demo](https://img.youtube.com/vi/IAFXhhgRYnk/maxresdefault.jpg)](https://www.youtube.com/watch?v=IAFXhhgRYnk)
