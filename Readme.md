# 3D Inventory System

A Unity 3D project built to explore how a fully three-dimensional inventory system
could work — where items are real physical objects you drag through the world,
not just icons in a UI panel.

The inventory chest lives inside the player camera, creating the illusion of a
fixed UI while keeping everything in 3D space. Items have weight, fall with
physics, and make different sounds depending on what they are.

## Controls & Gameplay
- WASD — move the player
- Q / E — rotate the camera
- Hold LMB + drag — pick up and move a physical item in the world
- Mouse Scroll while dragging — push or pull the item closer or further
- Hold LMB + RMB + move mouse — rotate a held item
- Drag item and pull it into the chest — adds it to the inventory
- I — open and close the inventory
- G — drop all the items from inventory (while it's open)
- LMB release over inventory item — drop the item back into the world

## Inventory Features
- Tracks total weight and item count against the slot limit
- Empty slots automatically sort and fill when items are removed
- Hover the chest to preview stored items as 2D icons
- Items spawn randomly into the scene up to a set limit

## License
This repository is for viewing purposes only. All content, including third-party assets, may not be reused, copied, or redistributed without explicit permission.

## Watch the Demo on YouTube
[![Watch the demo](https://img.youtube.com/vi/IAFXhhgRYnk/maxresdefault.jpg)](https://www.youtube.com/watch?v=IAFXhhgRYnk)
