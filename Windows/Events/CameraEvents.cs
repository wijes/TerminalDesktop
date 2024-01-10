using UnityEngine.EventSystems;

namespace TerminalDesktopMod
{
    public class CameraEvents: WindowEvents
    {
        public EventTrigger EventTriggerCamera;
        public EventTrigger EventTriggerSwitchPlayerLeft;
        public EventTrigger EventTriggerSwitchPlayerRight;
        public EventTrigger EventTriggerSwitchNightVision;
        public EventTrigger EventTriggerRadarPing;

        public EventTrigger.TriggerEvent SwitchPlayerLeftEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent SwitchPlayerRightEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent SwitchNightVisionEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent RadarPingEvent { get; set; } = new EventTrigger.TriggerEvent();
        public override void CreateUIEvents(DesktopWindowBase desktopWindowBase)
        {
            base.CreateUIEvents(desktopWindowBase);
            if (desktopWindowBase is not CameraWindow cameraWindow)
                return;
            var clickContentCamera = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = WindowClickEvent
            };
            DragWindowEvent.AddListener(desktopWindowBase.ClickWindow);
            EventTriggerCamera.triggers.Add(clickContentCamera);
            
            var clickSwitchPlayerLeft = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = SwitchPlayerLeftEvent
            };
            SwitchPlayerLeftEvent.AddListener(cameraWindow.SwitchPlayerLeft);
            EventTriggerSwitchPlayerLeft.triggers.Add(clickSwitchPlayerLeft);
            
            var clickSwitchPlayerRight = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = SwitchPlayerRightEvent
            };
            SwitchPlayerRightEvent.AddListener(cameraWindow.SwitchPlayerRight);
            EventTriggerSwitchPlayerRight.triggers.Add(clickSwitchPlayerRight);
            
            var clickSwitchNightVision = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = SwitchNightVisionEvent
            };
            SwitchNightVisionEvent.AddListener(cameraWindow.SwitchNightVision);
            EventTriggerSwitchNightVision.triggers.Add(clickSwitchNightVision);
            
            var clickRadarPing = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = RadarPingEvent
            };
            RadarPingEvent.AddListener(cameraWindow.RadarPing);
            EventTriggerRadarPing.triggers.Add(clickRadarPing);
        }
    }
}