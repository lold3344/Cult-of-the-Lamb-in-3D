# COTL in 3D

A first-person camera mod for the PC version of *Cult of the Lamb*.

The lamb and every other character stay as their original flat sprites. The mod moves the game camera to the player, switches it from orthographic to perspective, and adds independent mouse look.

## What it changes

After `CameraFollowTarget.LateUpdate`, the plugin:

- places the camera near `PlayerFarming`;
- reads the facing direction from `PlayerFarming.AimAngle`;
- disables orthographic projection;
- applies mouse yaw and pitch on top of that facing direction.

WASD movement is unchanged. Looking around does not turn the lamb by itself.

This is a camera experiment, not a conversion of the game into a 3D character game. Sprites can still look like cards when viewed edge-on.

## Requirements

- *Cult of the Lamb* for Windows
- [BepInEx 5](https://github.com/BepInEx/BepInEx) for Unity Mono, x64
- .NET Framework 4.x developer tools if you want to build it

The plugin version is 1.7.0.

## Install

1. Install BepInEx into the game directory and run the game once.
2. Build `src/Plugin.cs` into `CotlCameraMove.dll`.
3. Copy `config/local.cotl.camera-move.cfg` to `BepInEx/config/`.
4. Copy that DLL to:

```text
Cult of the Lamb/BepInEx/plugins/CotlCameraMove.dll
```

5. Restart the game.

The first launch creates:

```text
BepInEx/config/local.cotl.camera-move.cfg
```

## Configuration

| Setting | Default | Effect |
| --- | ---: | --- |
| `EyeLift` | `2.2` | Camera height above the floor, applied as negative Z |
| `EyeForward` | `0.45` | Moves the camera forward so the player sprite stays behind it |
| `Fov` | `70` | Perspective field of view |
| `AimOffset` | `0` | Extra degrees added to the lamb's facing angle |

Restart the game after editing the file.

## Build

Run this from the repository root after replacing the two paths with your local installs:

```bat
csc /nologo /target:library /optimize ^
  /out:CotlCameraMove.dll ^
  /reference:"GAME\Cult Of The Lamb_Data\Managed\netstandard.dll" ^
  /reference:"GAME\Cult Of The Lamb_Data\Managed\UnityEngine.dll" ^
  /reference:"GAME\Cult Of The Lamb_Data\Managed\UnityEngine.CoreModule.dll" ^
  /reference:"GAME\Cult Of The Lamb_Data\Managed\UnityEngine.InputLegacyModule.dll" ^
  /reference:"BEPINEX\BepInEx\core\BepInEx.dll" ^
  /reference:"BEPINEX\BepInEx\core\0Harmony.dll" ^
  src\Plugin.cs
```

`csc.exe` is included with .NET Framework, normally at:

```text
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
```

## Uninstall

Delete `BepInEx/plugins/CotlCameraMove.dll`.

To remove the loader as well, delete `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version`, and the `BepInEx` directory from the game folder.

## Status

Version 1.7.0 was playtested for camera placement, upright orientation, mouse look, and non-inverted WASD movement. It has not had a full playthrough.

## License

Apache License 2.0. See [LICENSE](LICENSE).

*Cult of the Lamb* and its assets belong to their respective owners. This repository contains no game assets.
