using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace TerminalDesktopMod
{
    public class TerminalWindow : DesktopWindowBase
    {
        public RawImage TerminalRawImage;
        public static Camera TerminalUICamera { get; set; }
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

        private void FixedUpdate()
        {
            ReferencesStorage.Terminal.terminalUIScreen.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
        }
        /// <summary>
        /// cleaning only when before leaving
        /// </summary>
        public static void ManagerDestroyed()
        {
            if (TerminalUICamera is not null)
            {
                Destroy(TerminalUICamera.gameObject);
                TerminalUICamera = null;
                OldTerminalTexture.Release(); 
            }
        }
        /// <summary>
        /// Need for old console
        /// </summary>
        private void CreateTerminalCamera()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            var terminalUICameraObj = Instantiate(player.gameplayCamera.gameObject, null);
            terminalUICameraObj.transform.position = new Vector3(0, -2000, 0);
            foreach (Transform child in terminalUICameraObj.transform)
                Destroy(child.gameObject);
            Destroy(terminalUICameraObj.GetComponent<AudioListener>());
            TerminalUICamera = terminalUICameraObj.GetComponent<Camera>();
            TerminalUICamera.depth = -5;
            TerminalUICamera.clearFlags = CameraClearFlags.SolidColor;
            
            var terminal = ReferencesStorage.Terminal;
            //var texSize = Mathf.Min(terminal.playerScreenTex.width, terminal.playerScreenTex.height);
            var texSize = 500; // average value for the standard player texture
            OldTerminalTexture = new RenderTexture(texSize, texSize, 1, GraphicsFormat.R8G8B8A8_UNorm);
            TerminalUICamera.targetTexture = OldTerminalTexture;

            terminal.terminalUIScreen.worldCamera = TerminalUICamera;
            terminal.terminalUIScreen.renderMode = RenderMode.ScreenSpaceCamera;
            terminal.terminalUIScreen.planeDistance = 5;
            
            
            var background = Instantiate(terminal.terminalUIAnimator.transform.Find("Image").gameObject, terminal.terminalUIScreen.transform);
            var backgroundRect = (RectTransform)background.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = Vector2.zero;
            backgroundRect.localScale = Vector2.one;
            backgroundRect.localPosition = Vector3.zero;
            backgroundRect.SetAsFirstSibling();
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.enabled = true;
            backgroundImage.color = Color.black;

            TerminalUICamera.fieldOfView = 80;
            TerminalUICamera.cullingMask = 1;
        }
    }
}