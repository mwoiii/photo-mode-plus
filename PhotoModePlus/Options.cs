using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System.Reflection;
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

        public static BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public static ConfigEntry<bool> disableIndicators;

        public static ConfigEntry<int> buttonPlacement;

        public static void Init() {
            disableIndicators = PhotoModePlus.config.Bind("Misc", "Disable Ping Indicators", true, "Hide pings while photo mode is active.");

            buttonPlacement = PhotoModePlus.config.Bind("Misc", "Photo Mode Button Index", 1, "The index position of the photo mode button in the pause menu.");

            if (rooEnabled) {
                RoOInit();
            }
        }

        private static void RoOInit() {
            ModSettingsManager.AddOption(new CheckBoxOption(disableIndicators, new CheckBoxConfig()));

            ModSettingsManager.AddOption(new IntSliderOption(buttonPlacement, new IntSliderConfig() { min = 0, max = 6 }));

            ModSettingsManager.SetModDescription("Config options for PhotoModePlus.");

            ModSettingsManager.SetModIcon(Assets.assetBundle.LoadAsset<Sprite>("icon"));
        }
    }
}