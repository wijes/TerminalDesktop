using UnityEngine.EventSystems;

namespace TerminalDesktopMod
{
    public class MineEvents: WindowEvents
    {
        public EventTrigger EventTriggerChangePower;

        public EventTrigger.TriggerEvent ChangePowerWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public override void CreateUIEvents(DesktopWindowBase desktopWindowBase)
        {
            base.CreateUIEvents(desktopWindowBase);
            if (desktopWindowBase is not MineWindow mineWindow)
                return;
            var pointUp  = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerUp,
                callback = ChangePowerWindowEvent
            };
            ChangePowerWindowEvent.AddListener(mineWindow.ChangeMinePower);
            EventTriggerChangePower.triggers.Add(pointUp);
        }
    }
}