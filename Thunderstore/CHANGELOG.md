## 1.0.0

- Initial release
  - Attempted to fix an NRE that could occur from a missing CameraRigController instance
  - Removed instantiation of empty GameObjects every time photo mode is entered
  - Entering and exiting photo mode now accurately preserves the timescale
  - Fixed the camera snapping straight down if you entered photo mode looking up by any measure
  - Fixed the button index option not placing the button at the expected index position
  - Fixed the disable indicators option breaking indicators for the rest of the stage
  - Added support for Risk of Options