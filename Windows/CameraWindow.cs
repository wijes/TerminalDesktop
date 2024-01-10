using GameNetcodeStuff;
using TerminalDesktopMod.Sync;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TerminalDesktopMod
{
    public class CameraWindow : DesktopWindowBase
    {
        public RawImage TerminalRawImage;
        public TextMeshProUGUI TargetName;
        private static GameObject BaseMapCamera { get; set; }
        private Camera MapCamera { get; set; }
        private RenderTexture CameraTexture { get; set; }
        private int CameraTargetIndex { get; set; }
        private bool UseNightVision { get; set; }
        private ManualCameraRenderer ManualCameraRenderer { get; set; }
        private Volume NightVisionVolume { get; set; }
        private float CameraOffsetY { get; set; } = 3.65f;

        protected override void Awake()
        {
            base.Awake();
            ManualCameraRenderer = ReferencesStorage.ManualCameraRenderer;
            CreateCamera();
        }

        protected override void Start()
        {
            base.Start();
            TerminalDesktopManager.Instance.ChangeUseEnergy(1);
            TerminalRawImage.texture = CameraTexture;
            TargetName.text = ManualCameraRenderer.radarTargets[CameraTargetIndex].name;
        }
        private void FixedUpdate()
        {
            CameraTargetIndex = Mathf.Clamp(CameraTargetIndex,0, ManualCameraRenderer.radarTargets.Count - 1);
            var target = ManualCameraRenderer.radarTargets[CameraTargetIndex].transform.position;
            MapCamera.transform.position = new Vector3(target.x, target.y + CameraOffsetY, target.z);
        }
        public override void CloseWindow()
        {
            base.CloseWindow();
            TerminalDesktopManager.Instance.ChangeUseEnergy(-1);
            CameraTexture.Release();
            Destroy(MapCamera.gameObject);
        }

        private void OnDestroy()
        {
            BaseMapCamera = null;
        }

        public void SwitchNightVision(BaseEventData baseEventData)
        {
            if (!UseNightVision && TerminalDesktopManager.Instance.GetFreeEnergy() <= 0)
            {
                TerminalDesktopManager.Instance.AddNotificationWindow("not enough energy for night mode");
                return;
            }
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomBool = true,
                CustomBool = !UseNightVision
            });
        }
        public void RadarPing(BaseEventData baseEventData)
        {
            var radar = ManualCameraRenderer.radarTargets[CameraTargetIndex].transform.gameObject.GetComponent<RadarBoosterItem>();
            if (!radar)
                return;
            radar.PlayPingAudioAndSync();
        }
        public void SwitchPlayerLeft(BaseEventData baseEventData)
        {
            var newIndex = GetNextIndexSwitchPlayer(-1);
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomInt = true,
                CustomInt = newIndex
            });
        }
        public void SwitchPlayerRight(BaseEventData baseEventData)
        {
            var newIndex = GetNextIndexSwitchPlayer(1);
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncCustomInt = true,
                CustomInt = newIndex
            });
        }
        private int GetNextIndexSwitchPlayer(int direction)
        {
            for (int i = 1; i < ManualCameraRenderer.radarTargets.Count; i++)
            {
                var newIndex = CameraTargetIndex + (i * direction);
                if (newIndex < 0)
                    newIndex = ManualCameraRenderer.radarTargets.Count + newIndex;
                if (newIndex >= ManualCameraRenderer.radarTargets.Count)
                    newIndex -= ManualCameraRenderer.radarTargets.Count;
                var target = ManualCameraRenderer.radarTargets[newIndex];
                var targetObj = ManualCameraRenderer.radarTargets[newIndex].transform.gameObject;
                
                if (!targetObj.activeSelf)
                    continue;
                if (target.isNonPlayer)
                    return newIndex;
                
                var player = targetObj.GetComponent<PlayerControllerB>();
                if (player.isPlayerControlled)
                    return newIndex;
            }

            return 0;
        }
        private void CreateCamera()
        {
            if (BaseMapCamera is null)
            {
                BaseMapCamera = GameObject.Find("MapCamera");
                var volumeCollider = BaseMapCamera.GetComponentInChildren<BoxCollider>();
                volumeCollider.size = Vector3.zero;// needed for night attention
            }

            var cameraObj = Instantiate(BaseMapCamera, null);
            NightVisionVolume = cameraObj.GetComponentInChildren<Volume>();
            
            MapCamera = cameraObj.GetComponent<Camera>();
            MapCamera.nearClipPlane = -2.47f;
            var terminal = ReferencesStorage.Terminal;
            CameraTexture = new RenderTexture((int)(terminal.playerScreenTex.width * 0.7f), (int)(terminal.playerScreenTex.height * 0.7f),
                1, GraphicsFormat.R8G8B8A8_UNorm);
            MapCamera.targetTexture = CameraTexture;
        }

        public override void WindowSync(WindowSync windowSync)
        {
            base.WindowSync(windowSync);
            if (windowSync.SyncCustomInt)
            {
                CameraTargetIndex = windowSync.CustomInt;
                TargetName.text = ManualCameraRenderer.radarTargets[CameraTargetIndex].name;
            }
            if (windowSync.SyncCustomBool)
            {
                UseNightVision = windowSync.CustomBool;
                NightVisionVolume.weight = UseNightVision ? 0 : 1;
                TerminalDesktopManager.Instance.ChangeUseEnergy(UseNightVision ? 1 : -1);
            }
        }

        public override WindowSync GetFullWindowSync()
        {
            return new WindowSync()
            {
                ChangeCollapsed = true,
                IsCollapsed = WindowCanvasGroup.alpha == 0,
                SyncPosition = true,
                Position = transform.localPosition,
                SyncScale = true,
                Scale = WindowContainer.sizeDelta,
                SyncCustomInt = true,
                CustomInt = CameraTargetIndex,
                SyncCustomBool = true,
                CustomBool = UseNightVision
            };
        }
    }
}