# 2D Roguelike Game (Assignment 2)

Unity Play link: https://play.unity.com/en/games/17e7c54b-a2fb-4117-b052-270ffb01c116/eat-or-die

## Gameplay
This is a turn-based 2D roguelike where the player moves one grid cell per turn.

- Each turn, the player’s **food decreases**.
- Collect food to **increase food** and survive longer.
- **Walls** block movement. Bumping a wall damages it until it is destroyed.
- **Enemies** block movement, attack if adjacent, and are destroyed when their HP reaches 0.
- Reaching the **exit cell** advances to the next level and generates a new board.
- If food reaches **0 or less**, the game ends (Game Over screen).

## Controls
- Move: **Arrow Keys**
- Pause: **Escape**
- Restart (after Game Over): **Enter**

## Implemented Bonus Features
- **Audio**: background music on the Main Camera (loop enabled) and SFX via `PlayOneShot()` (move, attack, food pickup, enemy attack, enemy death, game over).
- **Visual Effects**: Particle effects for wall destruction, food collection, and enemy death (played from code).
- **Smooth Cell Movement**: smooth coroutine-based sliding between cells; input is blocked during movement.
- **Object Pooling**: pooling for cell objects (walls, food, enemies) to avoid frequent Instantiate/Destroy.
- **Dynamic Board Scaling**: board size and enemy/wall counts increase as levels progress.
- **Main Menu + Pause Menu**: UI Toolkit screens for Play/Quit, Resume, Restart, and returning to the main menu.

## Built / Publishing Notes
- Built a WebGL version in Unity and uploaded it to Unity Play.
- The uploaded Unity Play version is the one linked above.

## Credits
- Project built following the Unity Learn “Create a 2D Roguelike Game” course.
- Uses the provided Roguelike2D tutorial assets (sprites, tiles, UI font).

