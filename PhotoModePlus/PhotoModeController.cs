using Rewired;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhotoModePlus {

    internal class PhotoModeController : MonoBehaviour, ICameraStateProvider {
        internal struct PhotoModeCameraState {
            internal float pitch;

            internal float yaw;

            internal float roll;

            internal Vector3 position;

            internal float fov;

            internal Quaternion Rotation {
                get {
                    return Quaternion.Euler(pitch, yaw, roll);
                }
                set {
                    Vector3 eulerAngles = value.eulerAngles;
                    pitch = eulerAngles.x;
                    yaw = eulerAngles.y;
                    roll = eulerAngles.z;
                }
            }
        }

        private static float cameraSpeed = 10f;

        private static float cameraSprintMultiplier = 5f;

        private static float cameraSlowMultiplier = 0.3f;

        public CameraRigController cameraRigController;

        private PhotoModeCameraState cameraState;

        private float currentTimescale;

        private float prevTimescale;

        private List<PingIndicator> pingIndicators;

        private Camera camera => cameraRigController.sceneCam;

        public MPEventSystem.InputSource currentInputSource { get; private set; }

        private bool gamepad => currentInputSource == MPEventSystem.InputSource.Gamepad;

        private event EmptyDelegate OnExit;

        internal void EnterPhotoMode(CameraRigController cameraRigController) {
            if (!cameraRigController) {
                Log.Error("Camera rig controller is null! Cancelling photo mode...");
                Destroy(gameObject);
            }

            this.cameraRigController = cameraRigController;
            if (this.cameraRigController) {
                this.cameraRigController.SetOverrideCam(this, 0f);
                this.cameraRigController.enableFading = false;
            }

            cameraState = default;
            cameraState.position = camera.transform.position;
            cameraState.Rotation = Quaternion.LookRotation(camera.transform.rotation * Vector3.forward, Vector3.up);
            cameraState.fov = camera.fieldOfView;

            // fix snapping from clamp
            if (cameraState.pitch > 90f) {
                cameraState.pitch -= 360f;
            }

            // pointless now but not pointless for future updates
            if (SettingsConVars.enableDamageNumbers.value) {
                SettingsConVars.enableDamageNumbers.SetBool(false);
                OnExit += () => {
                    SettingsConVars.enableDamageNumbers.SetBool(true);
                };
            }

            if (SettingsConVars.cvExpAndMoneyEffects.value) {
                SettingsConVars.cvExpAndMoneyEffects.SetBool(false);
                OnExit += () => {
                    SettingsConVars.cvExpAndMoneyEffects.SetBool(true);
                };
            }

            this.cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = false;
            OnExit += () => {
                this.cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = true;
            };

            if (Options.disableIndicators.Value) {
                SetIndicatorsVisible(false);
                OnExit += () => {
                    SetIndicatorsVisible(true);
                };
            }

            Player inputPlayer = this.cameraRigController.localUserViewer.inputPlayer;
            inputPlayer.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
            OnLastActiveControllerChanged(inputPlayer, inputPlayer.controllers.GetLastActiveController());
            if (PauseStopController.instance) {
                prevTimescale = PauseStopController.instance._oldTimeScale;
            } else {
                Log.Warning("PauseStopController instance is null! Recording previous timescale as 1.");
                prevTimescale = 1f;
            }
        }

        private void OnDisable() {
            if (cameraRigController) {
                Player inputPlayer = cameraRigController.localUserViewer.inputPlayer;
                inputPlayer.controllers.RemoveLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
            }
        }

        private void OnDestroy() {
            ExitPhotoMode();
        }

        private void OnLastActiveControllerChanged(Player player, Controller controller) {
            if (controller != null) {
                switch (controller.type) {
                    case ControllerType.Keyboard:
                        currentInputSource = MPEventSystem.InputSource.MouseAndKeyboard;
                        break;
                    case ControllerType.Mouse:
                        currentInputSource = MPEventSystem.InputSource.MouseAndKeyboard;
                        break;
                    case ControllerType.Joystick:
                        currentInputSource = MPEventSystem.InputSource.Gamepad;
                        break;
                }
            }
        }

        //private void SetupPhotoModeHUD(HUD hud) {
        //    // HELLO??????
        //    GameObject gameObject = hud.gameObject;
        //    GameObject gameObject2 = new GameObject("PhotoModeHUD", typeof(Canvas));
        //}

        private void ExitPhotoMode() {
            OnExit?.Invoke();
            if (cameraRigController) {
                cameraRigController.enableFading = true;
                cameraRigController.SetOverrideCam(null);
                camera.transform.localPosition = Vector3.zero;
                camera.transform.localRotation = Quaternion.identity;
            }

            for (int i = 0; i < PauseScreenController.instancesList.Count; i++) {
                PauseScreenController.instancesList[i].DestroyPauseScreen(shouldResumeOnDisable: true);
            }

            Time.timeScale = prevTimescale;
            Destroy(gameObject);
        }

        private void SetIndicatorsVisible(bool visible) {
            //if (Indicator.IndicatorManager.runningIndicators is List<Indicator> list) {
            //    for (int i = 0; i < Indicator.IndicatorManager.runningIndicators.Count; i++) {
            //        list[i].SetVisualizerInstantiated(visible);
            //    }
            //}

            if (!visible) {
                DamageNumberManager.instance.ps.Clear(true);
                cameraRigController.sprintingParticleSystem.Clear(withChildren: true);
                pingIndicators = new List<PingIndicator>(PingIndicator.instancesList);
            }

            for (int i = 0; i < pingIndicators.Count; i++) {
                if (pingIndicators[i]?.gameObject != null) {
                    pingIndicators[i].gameObject.SetActive(visible);
                }
            }

            cameraRigController.hud.combatHealthBarViewer.enabled = visible;
        }

        private void Update() {
            UserProfile userProfile = cameraRigController.localUserViewer.userProfile;
            Player inputPlayer = cameraRigController.localUserViewer.inputPlayer;
            if (inputPlayer.GetButton(25)) {
                ExitPhotoMode();
                return;
            }

            Time.timeScale = currentTimescale;

            float mouseLookSensitivity = userProfile.mouseLookSensitivity;
            float mouseLookScaleX = userProfile.mouseLookScaleX;
            float mouseLookScaleY = userProfile.mouseLookScaleY;
            float axis = inputPlayer.GetAxis(23);
            float axis2 = inputPlayer.GetAxis(24);
            float num = 1f;

            if (gamepad) {
                num = 10f;
            }

            if ((gamepad && inputPlayer.GetButton(9)) || Input.GetMouseButton(1)) {
                cameraState.fov = Mathf.Clamp(camera.fieldOfView + mouseLookSensitivity * Time.unscaledDeltaTime * axis2 * num, 4f, 120f);
            }

            if ((gamepad && inputPlayer.GetButton(10)) || Input.GetMouseButton(2)) {
                cameraState.roll += (0f - mouseLookScaleX) * mouseLookSensitivity * Time.unscaledDeltaTime * axis * num;
            } else {
                float xMovement = mouseLookScaleX * mouseLookSensitivity * Time.unscaledDeltaTime * axis * num;
                float yMovement = mouseLookScaleY * mouseLookSensitivity * Time.unscaledDeltaTime * axis2 * num;
                ConditionalNegate(ref xMovement, userProfile.mouseLookInvertX);
                ConditionalNegate(ref yMovement, userProfile.mouseLookInvertY);
                float f = cameraState.roll * (MathF.PI / 180f);
                cameraState.yaw += cameraState.fov * (xMovement * Mathf.Cos(f) - yMovement * Mathf.Sin(f));
                cameraState.pitch += cameraState.fov * ((0f - yMovement) * Mathf.Cos(f) - xMovement * Mathf.Sin(f));
                cameraState.pitch = Mathf.Clamp(cameraState.pitch, -90f, 90f);
            }

            Vector3 vector = new Vector3(inputPlayer.GetAxis(0) * num, 0f, inputPlayer.GetAxis(1) * num);
            if ((gamepad && inputPlayer.GetButton(7)) || Input.GetKey(KeyCode.Q)) {
                vector.y -= 1f;
            }

            if ((gamepad && inputPlayer.GetButton(8)) || Input.GetKey(KeyCode.E)) {
                vector.y += 1f;
            }

            vector.Normalize();

            if (inputPlayer.GetButton("Sprint")) {
                vector *= cameraSprintMultiplier;
            }

            if (Input.GetKey(KeyCode.LeftControl)) {
                vector *= cameraSlowMultiplier;
            }

            cameraState.position += cameraState.Rotation * vector * Time.unscaledDeltaTime * cameraSpeed;
            camera.transform.position = cameraState.position;
            Quaternion quaternion = cameraState.Rotation;
            if ((double)Mathf.Abs(quaternion.eulerAngles.z) < 2.0) {
                quaternion = quaternion.WithEulerAngles(null, null, 0f);
            }
            camera.transform.rotation = quaternion;
            camera.fieldOfView = cameraState.fov;
        }

        public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState) {
        }

        private void ConditionalNegate(ref float value, bool condition) {
            value = (condition ? (0f - value) : value);
        }

        public bool IsHudAllowed(CameraRigController cameraRigController) {
            return false;
        }

        public bool IsUserControlAllowed(CameraRigController cameraRigController) {
            return false;
        }

        public bool IsUserLookAllowed(CameraRigController cameraRigController) {
            return false;
        }
    }

}
