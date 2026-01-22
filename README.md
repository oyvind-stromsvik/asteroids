# Asteroids

A simple Asteroids clone I made just to see how it would feel to work on a project with a small scope and known boundaries. I still managed to have some feature creep, but I got it done in a fairly short amount of time.

The game is based on the classic Asteroids, but mostly on this: https://classicgaming.cc/classics/asteroids/index.php

The classic game mode replicates the original as closely as I bothered to make it. The custom game mode lets you tweak a few settings. There’s some additional settings available for tweaking in the project that I didn’t bother making available in-game.

Sounds effects are taken from the above link. All credits go to them. The only exception is the shield hit sound effect which I made myself.

The entire project is available for download by clicking the link on the right, no strings attached, do whatever you want with it.

I made a work in progress thread for this project on the official Unity forums if you’re interested in seeing that. It is probably the tightest most structured project I’ve done in Unity, which was cool. Sadly none of the old builds are available, but it may be of some interest anyway. Here’s a link to the thread: https://discussions.unity.com/t/asteroids-clone/558011

**Note:** There used to be server side highscores for this game. The code is included, but the backend no longer exists, so I commented out any parts that would break because of it.

## The Game

https://oyvind-stromsvik.github.io/asteroids/

### Controls

- **Left/Right** or **A/D**: Rotate ship
- **Up** or **W**: Thrust forward
- **CTRL** or **Spacebar**: Shoot
- **Shift**: Emergency teleport to random location

### Gameplay

1. Destroy asteroids by shooting them - they break into smaller pieces
2. Avoid collisions with asteroids and UFOs
3. Watch out for UFOs that appear periodically and shoot at you
4. Complete each level by destroying all asteroids
5. Earn extra lives by reaching score milestones (classic mode)

### Scoring

- Large asteroids: 20 points
- Medium asteroids: 50 points
- Small asteroids: 100 points
- UFO: 200 points

### Game Modes

#### Classic Mode
- Traditional lives system (3 lives to start)
- 4-bullet limit (original arcade limitation)
- No asteroid-to-asteroid collision

#### Custom Mode
- Choose between lives or recharging shields
- Adjust bullet limits and collision behavior
- Toggle visual effects

## Unity Project Requirements

- Unity 2022.3.62f3 or later
- Windows/Mac/Linux compatible

## Running The Project

1. Clone this repository:
   ```bash
   git clone git@github.com:oyvind-stromsvik/asteroids.git
   ```

2. Open the project in Unity 2022.3.62f3 or later

3. Open the scene at `Assets/Scenes/Game.unity`

4. Press Play in the Unity Editor or build the project for your target platform

## Project Structure

```
Assets/
├── Scenes/
│   └── Game.unity          # Main game scene
└── Scripts/
    ├── Asteroid.cs         # Asteroid behavior and destruction
    ├── DynamicObject.cs    # Base class for moving objects
    ├── DrawObject.cs       # Vector graphics rendering
    ├── GameManager.cs      # Core game logic and state management
    ├── Player.cs           # Player ship controls and behavior
    ├── Projectile.cs       # Bullet behavior
    ├── ServerScore.cs      # Online score submission
    ├── Shield.cs           # Shield visual effects
    ├── StarField.cs        # Background starfield animation
    └── Ufo.cs              # Enemy UFO behavior
```

## Technical Details

- **Graphics**: All game objects are rendered using Unity's LineRenderer component to create authentic vector graphics
- **Scoring**: Numbers are drawn using custom line renderer implementations based on ASCII codes
- **Music**: Dynamic tempo that increases as asteroids are destroyed
- **Physics**: Uses Unity's 2D physics system with custom collision handling

## Credits

Created by Øyvind Strømsvik

## License/Copyright

Sound effects are taken from classicgaming.cc (link at the bottom) so they belong to them, no idea what license applies to them to check that out for yourself if you intend to use the same sounds for whatever purpose. Everything else is made by me and you're free to do whatever you want with it.
