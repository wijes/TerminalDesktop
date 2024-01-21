using UnityEngine;
using static TerminalApi.Events.Events;
namespace TerminalDesktopMod
{
    public class SettingsIcon : DesktopIconBase
    {
        private bool UseStaticCamera { get; set; } = true;
        public Camera DynamicEventUICamera { get; private set; }
        public Camera StaticEventUICamera { get; private set; }
        public Transform StaticCameraPos { get; private set; }
        public Camera StaticCamera { get; private set; }
        private bool PreviousLocalVisorStatus { get; set; }

        protected override void Start()
        {
            base.Start();
            var container = TerminalDesktopManager.Instance.transform.Find("CameraContainer");
            StaticCameraPos = container.Find("TerminalStaticCamera");
            
            StaticEventUICamera = container.Find("TerminalStaticCameraEvent").GetComponent<Camera>();
            StaticEventUICamera.targetTexture = null;
            StaticEventUICamera.depth = 10;
            StaticEventUICamera.clearFlags = CameraClearFlags.Depth;
            StaticEventUICamera.gameObject.SetActive(false);

            TerminalBeganUsing += OnTerminalBeganUsing;
            TerminalDesktopManager.Instance.TerminalExitEvent.AddListener(OnTerminalExitedUsing);
        }

        private void FixedUpdate()
        {
            if (UseStaticCamera)
                return;
            var player = GameNetworkManager.Instance.localPlayerController;
            DynamicEventUICamera.fieldOfView = player.gameplayCamera.fieldOfView;
        }

        public override void Click()
        {
            base.Click();
            UseStaticCamera = !UseStaticCamera;
            SwitchCamera();
        }
        private void OnTerminalBeganUsing(object sender, TerminalEventArgs e)
        {
            SwitchCamera();
        }

        private void SwitchCamera()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            if (UseStaticCamera)
            {
                if (StaticCamera is null)
                    CreateStaticCamera();
                TerminalDesktopManager.Instance.CanvasDesktop.worldCamera = StaticEventUICamera;
                StaticCamera.depth = 50;
                PreviousLocalVisorStatus = player.localVisor.GetChild(0).gameObject.activeSelf;
                player.localVisor.GetChild(0).gameObject.SetActive(false);
                player.isClimbingLadder = false;
                player.ladderCameraHorizontal = 0; // set camera horizontal to center monitor
            }
            else
            {
                StaticCamera.depth = -20;
                if (DynamicEventUICamera is null)
                    CreateDynamicEventCamera();
                TerminalDesktopManager.Instance.CanvasDesktop.worldCamera = DynamicEventUICamera;
                player.localVisor.GetChild(0).gameObject.SetActive(PreviousLocalVisorStatus);
                player.isClimbingLadder = true; // free camera
                //player.ladderCameraHorizontal = 0; // set camera horizontal to center monitor
            }
        }
        private void OnTerminalExitedUsing()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            TerminalDesktopManager.Instance.CanvasDesktop.worldCamera = null;
            if (UseStaticCamera)
            {
                StaticCamera.depth = -20;
                player.localVisor.GetChild(0).gameObject.SetActive(PreviousLocalVisorStatus);
            }
            player.isClimbingLadder = false;
            player.ladderCameraHorizontal = 0; // set camera horizontal to center monitor
            player.gameplayCamera.transform.localRotation = Quaternion.Euler(0,0,0);
        }
        
        /// <summary>
        /// Need for world space UI
        /// </summary>
        private void CreateDynamicEventCamera()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            var eventUICameraObj = Instantiate(player.gameplayCamera.gameObject,
                player.gameplayCamera.transform);
            eventUICameraObj.transform.localPosition = Vector3.zero;
            eventUICameraObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Destroy(eventUICameraObj.GetComponent<AudioListener>());
            DynamicEventUICamera = eventUICameraObj.GetComponent<Camera>();
            DynamicEventUICamera.targetTexture = null;
            DynamicEventUICamera.depth = 10;
            DynamicEventUICamera.clearFlags = CameraClearFlags.Depth;
            eventUICameraObj.SetActive(false); // work event in disable state
            ReferencesStorage.Terminal.playerScreenTexHighRes.width = DynamicEventUICamera.pixelWidth;
            ReferencesStorage.Terminal.playerScreenTexHighRes.height = DynamicEventUICamera.pixelHeight;
        }
        private void CreateStaticCamera()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            var eventUICameraObj = Instantiate(player.gameplayCamera.gameObject,
                StaticCameraPos);
            eventUICameraObj.transform.localPosition = Vector3.zero;
            eventUICameraObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Destroy(eventUICameraObj.GetComponent<AudioListener>());
            StaticCamera = eventUICameraObj.GetComponent<Camera>();
            StaticCamera.targetTexture = ReferencesStorage.Terminal.playerScreenTexHighRes;
            StaticCamera.depth = -5;
            StaticCamera.clearFlags = CameraClearFlags.Depth;
            StaticCamera.fieldOfView = 58;
            StaticEventUICamera.fieldOfView = StaticCamera.fieldOfView - 2;
            ReferencesStorage.Terminal.playerScreenTexHighRes.width = StaticCamera.pixelWidth;
            ReferencesStorage.Terminal.playerScreenTexHighRes.height = StaticCamera.pixelHeight;
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.gameObject.SetActive(false);
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            TerminalBeganUsing -= OnTerminalBeganUsing;
        }
    }
}