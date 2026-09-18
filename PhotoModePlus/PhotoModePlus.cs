using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using RoR2.UI.SkinControllers;
using UnityEngine;

namespace PhotoModePlus {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class PhotoModePlus : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "mwmw";
        public const string PluginName = "PhotoModePlus";
        public const string PluginVersion = "1.1.0";

        public static ConfigFile config;

        public void Awake() {
            Log.Init(Logger);
            config = Config;
            Assets.Init();
            Options.Init();

            On.RoR2.UI.PauseScreenController.Awake += SetupPhotoModeButton;
        }

        private void SetupPhotoModeButton(On.RoR2.UI.PauseScreenController.orig_Awake orig, PauseScreenController self) {
            orig(self);

            GameObject button = self.GetComponentInChildren<ButtonSkinController>().gameObject;
            GameObject buttonCopy = Instantiate(button, button.transform.parent);
            buttonCopy.name = "GenericMenuButton (Photo Mode)";
            buttonCopy.SetActive(true);

            ButtonSkinController buttonSkinController = buttonCopy.GetComponent<ButtonSkinController>();
            buttonSkinController.GetComponent<LanguageTextMeshController>().token = "Photo Mode";

            HGButton hgButton = buttonCopy.GetComponent<HGButton>();
            hgButton.defaultFallbackButton = false;
            CameraRigController cameraRigInstance = CameraRigController.instancesList.Count > 0 ? CameraRigController.instancesList[^1] : null;
            hgButton.interactable = cameraRigInstance ? cameraRigInstance.localUserViewer != null : false;
            hgButton.onClick.AddListener(() => {
                GameObject photoMode = new GameObject("PhotoModeController");
                PhotoModeController photoModeController = photoMode.AddComponent<PhotoModeController>();
                photoModeController.EnterPhotoMode(cameraRigInstance);
            });
            buttonCopy.transform.SetSiblingIndex(Options.buttonPlacement.Value + 1);
        }
    }
}
