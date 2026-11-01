# Carnival Target Shooting — Labs 9 and 10


## Run the game

1. Open this project in Unity **6000.2.8f1** (Unity 6).
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press **Play**, then click the Game view to give it keyboard focus.

Move with **A/D** or the **left/right arrows**. Hold **Space** to shoot.
Press **R** to reset the score, ship, targets, and bullet pool.
You start with **10 shots**. Each enemy hit refills ammo to **10 shots**; misses do not
reload. If you run out, press **R** to restart with 10 shots and a fresh score.

The game has a black background and uses art I made being `Ship.png` for the
player, `Shoot.png` for bullets, and `Enemypng.png` for all three target types.
Targets move horizontally, bounce at the screen edges, and return one second
after a hit. They do not shoot back. There is no time limit or game-over screen.

## Save and load (Lab 10)

Press **S** to save and **L** to load. A message confirms the result.
There is one save slot, so saving again replaces it. Files persist after you
stop Play mode or close the game. Pressing **R** does not erase the saved game.

- `score.bin`: score stored as a four-byte integer using `BinaryWriter`,
  loaded using `BinaryReader`.
- `positions.json`: player and all three enemy positions, IDs, and visibility,
  written and read with Unity's built-in `JsonUtility`.

Both files are stored in `Application.persistentDataPath`. Saving prints the
full folder path in Unity's Console. No extra JSON package is needed.

`ISaveable` defines `SaveID`, `SavedData`, and `LoadFromData()`, following the
structure in the Week 10 slides. `TransformSaver` implements this interface
and captures/restores an object's position. `CarnivalGame` attaches it to the
player and each enemy with stable IDs (`player`, `enemy-0`, `enemy-1`, `enemy-2`).
`SavingService` saves those objects through the interface and matches IDs on
load, so object ordering and restarting the game do not change the mapping.
The score is saved separately in binary format.

Loading clears bullets in flight and restores ammo to 10; bullet positions,
ammo, enemy movement direction, and scenes are not saved. An enemy hidden at
save time is restored hidden and respawns after one second. All enemy positions
are included even if an enemy was temporarily hidden. Missing or invalid save
data shows a message without applying a partial load.

### Quick verification

1. Hit a few enemies, move the ship, and press **S**. Note the score and positions.
2. Move somewhere else, score more points, then press **L**.
3. Confirm the saved score and positions return (enemies immediately resume moving).
4. Stop and restart Play mode, then press **L** to verify persistence.
5. Inspect `positions.json` and confirm that `score.bin` is four bytes.

**Tools > Check Save and Load** runs automated editor checks using isolated
temporary files. It checks missing files, formats, all four positions, stable
IDs after recreating objects, visibility, overwriting, and invalid files.
It does not touch your actual game save.

## Design patterns

### Object Pool

`BulletPool.cs` creates 10 bullet objects at startup. `Fire()` activates an
available bullet and positions it at the ship. `Bullet.cs` moves it upward and
returns it to the pool after a hit or after leaving the screen. `Return()`
deactivates it so the same object can be reused. No bullets are created or
destroyed during shooting. Ammunition is tracked separately from the reusable
objects: firing spends one shot, and any enemy hit refills ammo to 10. Returning
a missed bullet recycles its object without restoring ammunition. Shooting is
blocked when ammunition is zero or all 10 objects are active.
The display shows remaining ammunition out of 10.

### Builder

`TargetBuilder.cs` constructs targets in steps with `WithSpeed()`, `WithSize()`,
and `WithPoints()`, followed by `Build()`. `CarnivalGame.cs` uses these steps
for three premade configurations:

- Large: size 1.5, speed 1, worth 10 points.
- Medium: size 1, speed 2, worth 20 points.
- Small: size 0.7, speed 3, worth 30 points.

All three use the same supplied enemy image. Smaller, faster targets earn more
points. `Build()` produces a separate configured `Target` object each time.

### Observer

Each `Target` publishes a `WasHit` C# event carrying its point value.
`ScoreManager` subscribes through `Observe()` and updates the score in its
`AddPoints()` callback. Targets do not reference `ScoreManager` or change the
score directly. The observer unsubscribes when destroyed. `CarnivalGame`
reads the resulting score for the display.

## Code structure

- `CarnivalGame`: scene setup, player movement, shooting input, display, reset.
- `BulletPool`: owns and reuses the 10 bullets and tracks ammunition.
- `Bullet`: projectile movement and swept collision checks against targets.
- `TargetBuilder`: step-by-step target construction.
- `Target`: movement, hit event, and delayed respawn.
- `ScoreManager`: observes target hits and tracks points.
- `ISaveable`: contract for capturing and restoring saved objects.
- `TransformSaver`: saves and loads a player or enemy's position and visibility.
- `SavingService`: writes and reads the binary score and JSON position files.

The scene's **Carnival Game** object already has all three image references.
The ship, pool, and targets are created when Play starts; no prefab setup is needed.


