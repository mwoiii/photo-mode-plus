## 1.1.3

- Fixed the scoreboard post-processing being possible to activate in Photo Mode since DLC4

## 1.1.2

- DLC4 fix

## 1.1.1

- Fixed certain controls being dependent on framerate

## 1.1.0

- Added configurable keybinds for the following actions:
  - Move up
  - Move down
  - Move faster
  - Move slower
  - Step forward
  - Increase speed
  - Decrease speed
  - Toggle cursor
<br><br/>
  
- Camera default/fast/slow speeds are now configurable

- Fixed a bug where time would stay frozen if you entered Photo Mode next to an interactable and held inspect while leaving

## 1.0.1

- Added an option to prevent camera movements while the dev console is active. Enabled by default

## 1.0.0

- Initial release
  - Attempted to fix an NRE that could occur from a missing CameraRigController instance
  - Removed instantiation of empty GameObjects every time photo mode is entered
  - Entering and exiting photo mode now accurately preserves the timescale
  - Fixed the camera snapping straight down if you entered photo mode looking up by any measure
  - Fixed the button index option not placing the button at the expected index position
  - Fixed the disable indicators option breaking indicators for the rest of the stage
  - Added support for Risk of Options