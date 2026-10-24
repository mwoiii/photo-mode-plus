using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;

namespace PhotoModePlus {

    public class Options {

        private static bool? _rooEnabled;

        public static bool rooEnabled {
            get {
                if (_rooEnabled == null) {
                    _rooEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                }
                return (bool)_rooEnabled;
            }
        }

        public static ConfigEntry<KeyboardShortcut> cameraUpKey;
        public static ConfigEntry<KeyboardShortcut> cameraDownKey;
        public static ConfigEntry<KeyboardShortcut> cameraFastKey;
        public static ConfigEntry<KeyboardShortcut> cameraSlowKey;
        public static ConfigEntry<KeyboardShortcut> stepTime;
        public static ConfigEntry<KeyboardShortcut> increaseSpeedKey;
        public static ConfigEntry<KeyboardShortcut> decreaseSpeedKey;
        public static ConfigEntry<KeyboardShortcut> cursorToggleKey;
        public static ConfigEntry<KeyboardShortcut> adjustFOVKey;
        public static ConfigEntry<KeyboardShortcut> defaultFOVKey;

        public static ConfigEntry<float> defaultSpeed;
        public static ConfigEntry<float> fastMult;
        public static ConfigEntry<float> slowMult;

        public static ConfigEntry<bool> disableIndicators;

        public static ConfigEntry<int> buttonPlacement;
        public static ConfigEntry<bool> disableCameraWithConsole;

        public static void Init() {
            cameraUpKey = PhotoModePlus.config.Bind("Keybinds", "Move Up Key", new KeyboardShortcut(KeyCode.E), "Key to hold to make the camera move up.");
            cameraDownKey = PhotoModePlus.config.Bind("Keybinds", "Move Down Key", new KeyboardShortcut(KeyCode.Q), "Key to hold to make the camera move down.");
            cameraFastKey = PhotoModePlus.config.Bind("Keybinds", "Camera Fast Key", new KeyboardShortcut(KeyCode.LeftControl), "Key to hold to make the camera move faster.");
            cameraSlowKey = PhotoModePlus.config.Bind("Keybinds", "Camera Slow Key", new KeyboardShortcut(KeyCode.LeftAlt), "Key to hold to make the camera move slower.");
            stepTime = PhotoModePlus.config.Bind("Keybinds", "Step Forward Key", new KeyboardShortcut(KeyCode.RightArrow), "Key to press to step forward one physics update.");
            increaseSpeedKey = PhotoModePlus.config.Bind("Keybinds", "Increase Default Speed Key", new KeyboardShortcut(KeyCode.UpArrow), "Key to press to increase the default speed of the current camera instance.");
            decreaseSpeedKey = PhotoModePlus.config.Bind("Keybinds", "Decrease Default Speed Key", new KeyboardShortcut(KeyCode.DownArrow), "Key to press to decrease the default speed of the current camera instance.");
            cursorToggleKey = PhotoModePlus.config.Bind("Keybinds", "Cursor Opener Toggle Key", new KeyboardShortcut(KeyCode.Tab), "Key to press to free and unfree the cursor while in Photo Mode. Freeing the cursor freezes movement controls.");
            adjustFOVKey = PhotoModePlus.config.Bind("Keybinds", "Adjust FOV Key", new KeyboardShortcut(KeyCode.F), "Key to hold to adjust the FOV while in Photo Mode.");
            defaultFOVKey = PhotoModePlus.config.Bind("Keybinds", "Reset FOV Key", new KeyboardShortcut(KeyCode.R), "Key to press to snap the FOV back to the default while in Photo Mode.");

            defaultSpeed = PhotoModePlus.config.Bind("Movement", "Default Camera Speed", 10f, "The speed at which the camera will move upon entering Photo Mode, without holding any keybinds.");
            fastMult = PhotoModePlus.config.Bind("Movement", "Fast Mult", 5f, "The value which the default camera speed is multiplied by when holding down the Fast Camera keybind.");
            slowMult = PhotoModePlus.config.Bind("Movement", "Slow Mult", 0.3f, "The value which the default camera speed is multiplied by when holding down the Slow Camera keybind.");

            disableIndicators = PhotoModePlus.config.Bind("Visuals", "Disable Ping Indicators", true, "Hide pings while Photo Mode is active.");

            buttonPlacement = PhotoModePlus.config.Bind("Misc", "Photo Mode Button Index", 1, "The index position of the Photo Mode button in the pause menu.");
            disableCameraWithConsole = PhotoModePlus.config.Bind("Misc", "Disable Camera Movements with Console", true, "Prevent the camera from moving in Photo Mode while the dev console is active.");

            if (rooEnabled) {
                RoOInit();
            }
        }

        private static void RoOInit() {
            ModSettingsManager.AddOption(new KeyBindOption(cameraUpKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(cameraDownKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(cameraFastKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(cameraSlowKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(stepTime, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(increaseSpeedKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(decreaseSpeedKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(cursorToggleKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(adjustFOVKey, new KeyBindConfig()));
            ModSettingsManager.AddOption(new KeyBindOption(defaultFOVKey, new KeyBindConfig()));

            ModSettingsManager.AddOption(new StepSliderOption(defaultSpeed, new StepSliderConfig() { min = 0.1f, max = 100f, increment = 0.1f }));
            ModSettingsManager.AddOption(new StepSliderOption(fastMult, new StepSliderConfig() { min = 1f, max = 100f, increment = 0.1f }));
            ModSettingsManager.AddOption(new StepSliderOption(slowMult, new StepSliderConfig() { min = 0f, max = 1f, increment = 0.01f }));

            ModSettingsManager.AddOption(new CheckBoxOption(disableIndicators, new CheckBoxConfig()));

            ModSettingsManager.AddOption(new IntSliderOption(buttonPlacement, new IntSliderConfig() { min = 0, max = 6 }));
            ModSettingsManager.AddOption(new CheckBoxOption(disableCameraWithConsole, new CheckBoxConfig()));

            ModSettingsManager.SetModDescription("Config options for PhotoModePlus.");

            ModSettingsManager.SetModIcon(Assets.assetBundle.LoadAsset<Sprite>("icon"));
        }
    }
}