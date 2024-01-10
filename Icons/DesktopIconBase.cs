using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TerminalDesktopMod
{
    public class DesktopIconBase : MonoBehaviour
    {
        public Vector2 DesktopNormalizedPosition;
        public UnityEvent ClickEvent { get; set; } = new UnityEvent();

        private RectTransform CanvasRectTransform { get; set; }
        private Image Image { get; set; }

        protected virtual void Awake()
        {
            CanvasRectTransform = (RectTransform)GetComponentInParent<Canvas>().transform;
            Image = GetComponentInParent<Image>();
        }
        protected virtual void Start()
        {
            ChangePosition(DesktopNormalizedPosition);
        }
        public virtual void Click()
        {
            ClickEvent.Invoke();
        }

        public virtual void ChangePosition(float xNormalized, float yNormalized)
        {
            ChangePosition(new Vector2(xNormalized, yNormalized));
        }
        public virtual void ChangePosition(Vector2 newNormalizedPos)
        {
            var iconRect = (transform as RectTransform);
            var newPos = CanvasRectTransform.sizeDelta * newNormalizedPos;
            newPos.y *= -1;
            iconRect.anchoredPosition = newPos;
        }
        public virtual void ChangeIconSprite(Sprite iconSprite)
        {
            Image.sprite = iconSprite;
        }
    }
}