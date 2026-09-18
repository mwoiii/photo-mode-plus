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

        private float cameraSpeed;

        public CameraRigController cameraRigController;

        private InteractionDriver interactionDriver;

        private PhotoModeCameraState cameraState;

        private Player inputPlayer;

        private UserProfile userProfile;

        private float? prevTimescale;

        private bool isSteppingTime;

        private float stepTimeHoldDuration;

        private List<PingIndicator> pingIndicators;

        private event EmptyDelegate OnExit;

        private CursorOpener cursorOpener;

        private bool _cursorActive;

        private bool cursorActive {
            get {
                return _cursorActive;
            }
            set {
                if (!cursorOpener) {
                    gameObject.AddComponent<MPEventSystemProvider>().fallBackToMainEventSystem = true;
                    gameObject.AddComponent<MPEventSystemLocator>();
                    cursorOpener = gameObject.AddComponent<CursorOpener>();
                }
                cursorOpener.enabled = value;
                _cursorActive = value;
            }
        }

        private Camera camera => cameraRigController.sceneCam;

        public MPEventSystem.InputSource currentInputSource { get; private set; }

        private bool gamepad => currentInputSource == MPEventSystem.InputSource.Gamepad;

        internal void EnterPhotoMode(CameraRigController cameraRigController) {
            cameraSpeed = Options.defaultSpeed.Value;

            if (!cameraRigController) {
                Log.Error("Camera rig controller is null! Cancelling photo mode...");
                Destroy(gameObject);
                return;
            }

            userProfile = cameraRigController.localUserViewer.userProfile;

            SetupCameraState(cameraRigController);

            SetInteractionDriverActive(false); // doing this because inspection can cause problems. avoiding it outright

            HoldVisuals();
        }

        private void SetInteractionDriverActive(bool active) {
            if (!interactionDriver && !(cameraRigController.targetBody && cameraRigController.targetBody.TryGetComponent(out interactionDriver))) {
                return;
            }

            interactionDriver.enabled = active;
            if (!active) {
                interactionDriver.SetCurrentInteractable(null);
            }
        }

        private void HoldVisuals() {
            cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = false;
            OnExit += () => {
                cameraRigController.hud.mainContainer.GetComponent<Canvas>().enabled = true;
            };

            if (Options.disableIndicators.Value) {
                SetIndicatorsVisible(false);
                OnExit += () => {
                    SetIndicatorsVisible(true);
                };
            }
        }

        private void SetupCameraState(CameraRigController cameraRigController) {
            this.cameraRigController = cameraRigController;
            this.cameraRigController.SetOverrideCam(this, 0f);
            this.cameraRigController.enableFading = false;

            cameraState = default;
            cameraState.position = camera.transform.position;
            cameraState.Rotation = Quaternion.LookRotation(camera.transform.rotation * Vector3.forward, Vector3.up);
            cameraState.fov = camera.fieldOfView;

            // fix snapping from clamp
            if (cameraState.pitch > 90f) {
                cameraState.pitch -= 360f;
            }

            inputPlayer = cameraRigController.localUserViewer.inputPlayer;
            inputPlayer.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
            OnLastActiveControllerChanged(inputPlayer, inputPlayer.controllers.GetLastActiveController());
            if (PauseStopController.instance) {
                prevTimescale = PauseStopController.instance._oldTimeScale;
            } else {
                prevTimescale = 1f;
            }
        }

        private void ExitPhotoMode() {
            OnExit?.Invoke();

            SetInteractionDriverActive(true);

            DestroyCameraState();
        }

        private void DestroyCameraState() {
            if (cameraRigController) {
                cameraRigController.enableFading = true;
                cameraRigController.SetOverrideCam(null);
                camera.transform.localPosition = Vector3.zero;
                camera.transform.localRotation = Quaternion.identity;
            }

            for (int i = 0; i < PauseScreenController.instancesList.Count; i++) {
                PauseScreenController.instancesList[i].DestroyPauseScreen(true);
            }

            if (PauseStopController.instance) {
                PauseStopController.instance.Pause(false);
            }

            if (prevTimescale != null) {
                Time.timeScale = (float)prevTimescale;
            }

            Destroy(gameObject);
        }

        private void SetIndicatorsVisible(bool visible) {
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

        private void Update() {
            if (isSteppingTime) {
                Time.timeScale = 1f;
            } else {
                Time.timeScale = 0f;
            }

            if (inputPlayer.GetButton(25)) { // magic number idk. lots of magic numbers hereon from decomp
                ExitPhotoMode();
                return;
            }

            HandleUtilityInputs();

            if (cursorActive || (Options.disableCameraWithConsole.Value && ConsoleWindow.instance)) {
                return;
            }

            HandleMovementInputs();
        }

        private void FixedUpdate() {
            if (isSteppingTime) {
                isSteppingTime = false;
            }
        }

        private void HandleUtilityInputs() {
            const float speedIncrement = 0.5f;
            const float minSpeed = 0f;
            const float maxSpeed = 200f;
            const float stepHoldThreshold = 0.6f;

            if (Input.GetKey(Options.increaseSpeedKey.Value.MainKey)) {
                cameraSpeed = Mathf.Clamp(cameraSpeed + speedIncrement, minSpeed, maxSpeed);
            }

            if (Input.GetKey(Options.decreaseSpeedKey.Value.MainKey)) {
                cameraSpeed = Mathf.Clamp(cameraSpeed - speedIncrement, minSpeed, maxSpeed);
            }

            if (Input.GetKeyDown(Options.stepTime.Value.MainKey)) {
                isSteppingTime = true;
            }

            if (Input.GetKeyDown(Options.cursorToggleKey.Value.MainKey)) {
                cursorActive = !cursorActive;
            }

            if (Input.GetKey(Options.stepTime.Value.MainKey)) {
                stepTimeHoldDuration += Time.unscaledDeltaTime;
                if (stepTimeHoldDuration > stepHoldThreshold) {
                    isSteppingTime = true;
                }
            } else {
                stepTimeHoldDuration = 0f;
            }
        }

        private void HandleMovementInputs() {
            float mouseLookSensitivity = userProfile.mouseLookSensitivity;
            float mouseLookScaleX = userProfile.mouseLookScaleX;
            float mouseLookScaleY = userProfile.mouseLookScaleY;
            float axis = inputPlayer.GetAxis(23);
            float axis2 = inputPlayer.GetAxis(24);
            float activeGamepadMult = 1f;

            if (gamepad) {
                activeGamepadMult = 10f;
            }

            //if ((gamepad && inputPlayer.GetButton(9)) || Input.GetMouseButton(1)) {
            //    cameraState.fov = Mathf.Clamp(camera.fieldOfView + mouseLookSensitivity * Time.unscaledDeltaTime * axis2 * activeGamepadMult, 4f, 120f);
            //}

            if ((gamepad && inputPlayer.GetButton(10)) || Input.GetMouseButton(2)) {
                cameraState.roll += (0f - mouseLookScaleX) * mouseLookSensitivity * Time.unscaledDeltaTime * axis * activeGamepadMult * 10f;
            } else {
                float xMovement = mouseLookScaleX * mouseLookSensitivity * Time.unscaledDeltaTime * axis * activeGamepadMult;
                float yMovement = mouseLookScaleY * mouseLookSensitivity * Time.unscaledDeltaTime * axis2 * activeGamepadMult;
                ConditionalNegate(ref xMovement, userProfile.mouseLookInvertX);
                ConditionalNegate(ref yMovement, userProfile.mouseLookInvertY);
                float f = cameraState.roll * (MathF.PI / 180f);
                cameraState.yaw += cameraState.fov * (xMovement * Mathf.Cos(f) - yMovement * Mathf.Sin(f));
                cameraState.pitch += cameraState.fov * ((0f - yMovement) * Mathf.Cos(f) - xMovement * Mathf.Sin(f));
                cameraState.pitch = Mathf.Clamp(cameraState.pitch, -90f, 90f);
            }

            Vector3 movementInput = new Vector3(inputPlayer.GetAxis(0) * activeGamepadMult, 0f, inputPlayer.GetAxis(1) * activeGamepadMult);
            if ((gamepad && inputPlayer.GetButton(7)) || Input.GetKey(Options.cameraDownKey.Value.MainKey)) {
                movementInput.y -= 1f;
            }

            if ((gamepad && inputPlayer.GetButton(8)) || Input.GetKey(Options.cameraUpKey.Value.MainKey)) {
                movementInput.y += 1f;
            }

            movementInput.Normalize();

            if (Input.GetKey(Options.cameraFastKey.Value.MainKey)) {
                movementInput *= Options.fastMult.Value;
            }

            if (Input.GetKey(Options.cameraSlowKey.Value.MainKey)) {
                movementInput *= Options.slowMult.Value;
            }

            cameraState.position += cameraState.Rotation * movementInput * Time.unscaledDeltaTime * cameraSpeed;
            camera.transform.position = cameraState.position;
            Quaternion quaternion = cameraState.Rotation;
            if ((double)Mathf.Abs(quaternion.eulerAngles.z) < 2.0) {
                quaternion = quaternion.WithEulerAngles(null, null, 0f);
            }
            camera.transform.rotation = quaternion;
            //camera.fieldOfView = cameraState.fov;
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
