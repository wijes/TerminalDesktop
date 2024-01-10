using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TerminalDesktopMod
{
    public class NotificationWindow : DesktopWindowBase
    {
        [SerializeField] private TextMeshProUGUI NotigicationTextMeshProUGUI;
        protected override void Start()
        {
        }

        public override void EndMoveWindow(BaseEventData baseEventData)
        {
        }

        public override void EndScaleWindow (BaseEventData baseEventData)
        {
        }

        public override void ClickCloseWindow(BaseEventData baseEventData)
        {
            Destroy(gameObject);
        }

        public void SetText(string text)
        {
            NotigicationTextMeshProUGUI.text = text;
        }
    }
}