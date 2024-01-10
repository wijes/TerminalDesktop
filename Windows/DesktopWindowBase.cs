using TerminalDesktopMod.Sync;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TerminalDesktopMod
{
    public class DesktopWindowBase : MonoBehaviour
    {
        public int WindowIndex { get; set; } // used to identify the window
        public Transform WindowContentParent;
        public Image ScaleImage;
        public Sprite CollapseSprite;
        public CanvasGroup WindowCanvasGroup { get; set; }
        public bool ProportionalScale = true;
        public Vector2 MinWindowScale = new Vector2(50, 50);
        public Vector2 MaxWindowScale = new Vector2(500, 500);
        public RectTransform WindowContainer { get; set; }
        [HideInInspector] public WindowEvents WindowEvents;
        protected Canvas DesktopCanvas { get; set; }
        protected RectTransform DesktopCanvasRectTransform { get; set; }
        protected Vector2 StartScaleSize { get; set; }
        protected Vector2 StartMovePos { get; set; }
        protected Vector2 WinRightDownPos { get; set; }
        protected Vector2 WinLeftUpPos { get; set; }
        protected virtual void Awake()
        {
            WindowContainer = (RectTransform)transform;
            WindowEvents = GetComponent<WindowEvents>();
            DesktopCanvas = TerminalDesktopManager.Instance.CanvasDesktop;
            WindowCanvasGroup = GetComponent<CanvasGroup>();
            DesktopCanvasRectTransform = (RectTransform)DesktopCanvas.transform;
            WindowEvents.CreateUIEvents(this);
        }


        protected virtual void Start()
        {
        }

        public virtual void ClickWindow(BaseEventData baseEventData)
        {
            GetFocus();
        }

        public virtual void GetFocus()
        {
            WindowContainer.transform.SetAsLastSibling();
        }

        public virtual void StartMoveWindow(BaseEventData baseEventData)
        {
            GetFocus();
            UpdateWindowBounds();
            if (baseEventData is not PointerEventData pointerEventData)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DesktopCanvasRectTransform,
                pointerEventData.pressPosition,
                DesktopCanvas.worldCamera,
                out var positionStart);
            StartMovePos = positionStart;
        }

        public virtual void MoveWindow(BaseEventData baseEventData)
        {
            if (baseEventData is not PointerEventData pointerEventData)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DesktopCanvasRectTransform,
                pointerEventData.position,
                DesktopCanvas.worldCamera,
                out var position);
            var directionMove = position - StartMovePos;

            var newPos = WinLeftUpPos + directionMove;
            var maxX = DesktopCanvasRectTransform.sizeDelta.x * 0.5f;
            newPos.x = Mathf.Clamp(newPos.x, -maxX, maxX - WindowContainer.sizeDelta.x);

            var maxY = DesktopCanvasRectTransform.sizeDelta.y * 0.5f;
            newPos.y = Mathf.Clamp(newPos.y, -maxY + WindowContainer.sizeDelta.y, maxY);
            WindowContainer.position = DesktopCanvas.transform.TransformPoint(newPos);
        }
        public virtual void EndMoveWindow(BaseEventData baseEventData)
        {
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncPosition = true,
                Position = transform.localPosition
            });
        }
        public virtual void StartScaleWindow(BaseEventData baseEventData)
        {
            GetFocus();
            StartScaleSize = WindowContainer.sizeDelta;
            UpdateWindowBounds();
        }
        public virtual void ScaleWindow(BaseEventData baseEventData)
        {
            if (baseEventData is not PointerEventData pointerEventData)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DesktopCanvasRectTransform,
                pointerEventData.position,
                DesktopCanvas.worldCamera,
                out var position);

            var maxX = DesktopCanvasRectTransform.sizeDelta.x * 0.5f;
            position.x = Mathf.Clamp(position.x, -maxX, maxX);
            var maxY = DesktopCanvasRectTransform.sizeDelta.y * 0.5f;
            position.y = Mathf.Clamp(position.y, -maxY, maxY);

            var direction = (position - WinRightDownPos);
            direction.y *= -1;
            var newSize = StartScaleSize + direction;
            
            if (ProportionalScale) {
                var minSquare = Mathf.Min(newSize.x, newSize.y);
                newSize = new Vector2(minSquare, minSquare);
            }
            newSize.x = Mathf.Clamp(newSize.x, MinWindowScale.x, MaxWindowScale.x);
            newSize.y = Mathf.Clamp(newSize.y, MinWindowScale.y, MaxWindowScale.y);
            WindowContainer.sizeDelta = newSize;
        }
        public virtual void EndScaleWindow(BaseEventData baseEventData)
        {
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                SyncScale = true,
                Scale = WindowContainer.sizeDelta
            });
        }
        protected virtual void UpdateWindowBounds()
        {
            var screenPointDown = RectTransformUtility.WorldToScreenPoint(
                DesktopCanvas.worldCamera,
                ScaleImage.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DesktopCanvasRectTransform,
                screenPointDown,
                DesktopCanvas.worldCamera,
                out var winRightDownPos);
            WinRightDownPos = winRightDownPos;
            var screenPointUp = RectTransformUtility.WorldToScreenPoint(
                DesktopCanvas.worldCamera,
                WindowContainer.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                DesktopCanvasRectTransform,
                screenPointUp,
                DesktopCanvas.worldCamera,
                out var winLeftUpPos);
            WinLeftUpPos = winLeftUpPos;
        }
        public virtual void ClickCloseWindow(BaseEventData baseEventData)
        {
            TerminalDesktopManager.Instance.CloseWindow(this);
        }
        public virtual void CloseWindow()
        {
            
        }
        /// <summary>
        /// used only to release resources
        /// for everything else use CloseWindow
        /// </summary>
        private void OnDestroy()
        {
        }

        public virtual void ClickCollapseWindow(BaseEventData baseEventData)
        {
            TerminalDesktopManager.Instance.UpdateWindow(this, new WindowSync()
            {
                ChangeCollapsed = true,
                IsCollapsed = true
            });
        }
        public virtual void CollapseWindow()
        {
            WindowCanvasGroup.alpha = 0;
            WindowCanvasGroup.blocksRaycasts = false;
            WindowCanvasGroup.interactable = false;
            TerminalDesktopManager.Instance.CollapseWindow(this);
        }
        public virtual void ExpandWindow()
        {
            WindowCanvasGroup.alpha = 1;
            WindowCanvasGroup.blocksRaycasts = true;
            WindowCanvasGroup.interactable = true;
            GetFocus();
            TerminalDesktopManager.Instance.ExpandWindow(this);
        }
        public virtual void WindowSync(WindowSync windowSync)
        {
            if (windowSync.SyncPosition)
                transform.localPosition = windowSync.Position;
            if (windowSync.ChangeCollapsed)
            {
                if (windowSync.IsCollapsed)
                    CollapseWindow();
                else
                    ExpandWindow();
            }
            if (windowSync.SyncScale)
                WindowContainer.sizeDelta = windowSync.Scale;
        }
        /// <summary>
        /// For new client
        /// </summary>
        /// <param name="windowSync"></param>
        public virtual WindowSync GetFullWindowSync()
        {
            return new WindowSync()
            {
                ChangeCollapsed = true,
                IsCollapsed = WindowCanvasGroup.alpha == 0,
                SyncPosition = true,
                Position = transform.localPosition,
                SyncScale = true,
                Scale = WindowContainer.sizeDelta
            };
        }
    }
}