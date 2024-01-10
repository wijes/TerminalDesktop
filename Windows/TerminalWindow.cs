using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace TerminalDesktopMod
{
    public class TerminalWindow : DesktopWindowBase
    {
        public RawImage TerminalRawImage;
        private static Camera TerminalUICamera { get; set; }
        private static RenderTexture OldTerminalTexture { get; set; }
        protected override void Start()
        {
            base.Start();
            if (TerminalUICamera is null)
                CreateTerminalCamera();
            TerminalRawImage.texture = OldTerminalTexture;
        }

        public void InputFocus(BaseEventData baseEventData)
        {
            GetFocus();
            ReferencesStorage.Terminal.screenText.Select();
        }

        private void OnDestroy()
        {
            TerminalUICamera = null;
            OldTerminalTexture.Release();
        }

        /// <summary>
        /// Need for old console
        /// </summary>
        private void CreateTerminalCamera()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            var terminalUICameraObj = Instantiate(player.gameplayCamera.gameObject, null);
            terminalUICameraObj.transform.position = new Vector3(0, -1000, 0);
            foreach (Transform child in terminalUICameraObj.transform)
                Destroy(child.gameObject);
            Destroy(terminalUICameraObj.GetComponent<AudioListener>());
            TerminalUICamera = terminalUICameraObj.GetComponent<Camera>();
            TerminalUICamera.depth = -5;
            TerminalUICamera.clearFlags = CameraClearFlags.SolidColor;
            TerminalUICamera.fieldOfView = 10;
            var terminal = ReferencesStorage.Terminal;
            OldTerminalTexture = new RenderTexture(terminal.playerScreenTex.width, terminal.playerScreenTex.height, 1, GraphicsFormat.R8G8B8A8_UNorm);
            TerminalUICamera.targetTexture = OldTerminalTexture;

            terminal.terminalUIScreen.worldCamera = TerminalUICamera;
            terminal.terminalUIScreen.renderMode = RenderMode.ScreenSpaceCamera;
            terminal.terminalUIScreen.planeDistance = 1;
        }
    }
}